using k8s;
using k8s.Models;
using Microsoft.Extensions.Options;
using TrivyOperator.Dashboard.Application.Kubernetes.Models;
using TrivyOperator.Dashboard.Application.Kubernetes.WatcherState.Options;
using TrivyOperator.Dashboard.Application.Shared.Models;
using TrivyOperator.Dashboard.Domain.Kubernetes.ValueObjects;
using TrivyOperator.Dashboard.Infrastructure.Kubernetes.EventPipeline.Mappers;
using TrivyOperator.Dashboard.Infrastructure.Kubernetes.EventPipeline.Services.EventPublishers.Abstractions;
using TrivyOperator.Dashboard.Infrastructure.Kubernetes.EventPipeline.Services.ResourceWatches.Abstractions;
using TrivyOperator.Dashboard.Infrastructure.Kubernetes.EventPipeline.Services.WatchSessions.Abstractions;
using TrivyOperator.Dashboard.Infrastructure.Kubernetes.EventPipeline.Services.WatchSessions.Failures.Abstractions;

namespace TrivyOperator.Dashboard.Infrastructure.Kubernetes.EventPipeline.Services.WatchSessions;

// one list + watch loop for one key
// - the data is cleared (Flushed) only on 410 (the resourceVersion / continue token is gone) and on Stop
// - any other failure keeps the data and the resourceVersion, and the watch is resumed after a delay
// - a failure in the middle of the paged initial list retries the same page, when it is worth retrying in place
// - the watcher only publishes events (Error with the failure, Skipped for malformed objects); status, alerts
//   and metrics are decided by the processors
public sealed class KubernetesWatchSession<TKubernetesObjectList, TKubernetesObject>(
    ResourceLocation key,
    IKubernetesResourceWatch<TKubernetesObjectList, TKubernetesObject> resourceWatch,
    IKubernetesEventPublisher<TKubernetesObject> eventPublisher,
    IKubernetesWatchSessionThrottle throttle,
    IWatchFailureClassifier failureClassifier,
    IWatchRetryPolicy retryPolicy,
    IOptions<WatchersOptions> options,
    ILogger<KubernetesWatchSession<TKubernetesObjectList, TKubernetesObject>> logger
) : IDisposable
    where TKubernetesObject : class, IKubernetesObject<V1ObjectMeta>, new()
    where TKubernetesObjectList : IKubernetesObject<V1ListMeta>, IItems<TKubernetesObject>
{
    // a watch that ends without any event is not a failure, but it must not turn into a busy loop
    private static readonly TimeSpan EmptyWatchDelay = TimeSpan.FromSeconds(1);

    private readonly CancellationTokenSource cancellationTokenSource = new();

    // malformed objects already logged as warnings (uid / resourceVersion), to keep the log readable on re-lists
    private readonly HashSet<string> loggedMalformedObjects = [];

    private Task? runningTask;

    // the failure streak; reset by any sign of health (a listed page, a received watch event)
    private int consecutiveFailures;
    private DateTime failingSince;

    // an initial list published some pages and did not complete; the next list must start from a clean state
    private bool initialListIncomplete;

    public bool IsRunning =>
        runningTask is { IsCompleted: false };

    public void Start(CancellationToken ctx = default)
    {
        if (IsRunning)
        {
            logger.LogWarning(
                "Watch session for {kubernetesObjectType} and key {key} already running. Ignoring start request.",
                typeof(TKubernetesObject).Name,
                key
            );

            return;
        }

        CancellationToken linkedCancellationToken =
            CancellationTokenSource.CreateLinkedTokenSource(
                ctx,
                cancellationTokenSource.Token
            ).Token;

        runningTask = Run(linkedCancellationToken);
    }

    public async Task Stop(CancellationToken cancellationToken = default)
    {
        await cancellationTokenSource.CancelAsync();

        if (runningTask is null)
        {
            return;
        }

        try
        {
            await runningTask.WaitAsync(cancellationToken);
        }
        catch (OperationCanceledException) when (cancellationTokenSource.IsCancellationRequested)
        {
            // Expected when stopping the session.
        }

        // the session is gone, so drop what downstream processors hold for this key
        await eventPublisher.Publish(key, PipelineEventType.Flushed, cancellationToken);
    }

    private async Task Run(CancellationToken ctx)
    {
        string? resourceVersion = null;

        while (!ctx.IsCancellationRequested)
        {
            WatchPhase phase = WatchPhase.InitialList;

            try
            {
                if (string.IsNullOrEmpty(resourceVersion))
                {
                    if (initialListIncomplete)
                    {
                        // a previous list failed after publishing some pages; the new list replays everything,
                        // so drop the partial state first (the same situation as a 410: the state is incomplete)
                        await eventPublisher.Publish(key, PipelineEventType.Flushed, ctx);
                        initialListIncomplete = false;
                    }

                    resourceVersion = await throttle.Run(
                        ProcessInitialResources,
                        () => eventPublisher.Publish(key, PipelineEventType.Throttled, ctx),
                        ctx
                    );

                    logger.LogInformation(
                        "Initial Resources Processed - {kubernetesObjectType} - {key} - {resourceVersion}",
                        typeof(TKubernetesObject).Name,
                        key,
                        resourceVersion
                    );

                    await eventPublisher.Publish(key, PipelineEventType.Initialized, ctx);
                }

                phase = WatchPhase.WatchOpen;
                bool receivedAnyEvent = false;

                IAsyncEnumerable<WatchEvent<TKubernetesObject>> watchList =
                    resourceWatch.GetWatchList(key, resourceVersion, GetWatcherRandomTimeout(), ctx);

                await foreach (WatchEvent<TKubernetesObject> watchEvent in watchList)
                {
                    phase = WatchPhase.WatchStream;
                    receivedAnyEvent = true;

                    // every event moves the resourceVersion, so a resumed watch does not replay what was processed
                    resourceVersion = GetResourceVersion(watchEvent) ?? resourceVersion;

                    await PublishWatchEvent(watchEvent, ctx);

                    ResetFailures();
                }

                logger.LogDebug(
                    "Watch stopped - {kubernetesObjectType} - {key} - resuming from {resourceVersion}",
                    typeof(TKubernetesObject).Name,
                    key,
                    resourceVersion
                );

                if (!receivedAnyEvent)
                {
                    await Task.Delay(EmptyWatchDelay, ctx);
                }
            }
            catch (OperationCanceledException) when (ctx.IsCancellationRequested)
            {
                // Session is stopping.
                break;
            }
            catch (Exception ex)
            {
                WatchFailure failure = ClassifyNext(ex, phase);
                CommitFailure(failure);

                if (failure.Category == WatchFailureCategory.Gone)
                {
                    // the resourceVersion (or the continue token) is gone; the re-list replays the full state,
                    // so drop what downstream processors hold for this key
                    resourceVersion = null;

                    await eventPublisher.Publish(key, PipelineEventType.Flushed, ctx);
                    initialListIncomplete = false;
                }

                await eventPublisher.Publish(key, PipelineEventType.Error, ctx, failure: failure);

                if (!await DelayRetry(failure, ctx))
                {
                    break;
                }
            }
        }
    }

    private async Task<string> ProcessInitialResources(CancellationToken ctx = default)
    {
        string? continueToken = null;
        string? resourceVersion = null;

        do
        {
            ResourcePage<TKubernetesObject> page = await GetInitialResourcesPage(continueToken, ctx);

            // from here on, a failure leaves part of this list published
            initialListIncomplete = true;

            foreach (TKubernetesObject item in page.Items)
            {
                await eventPublisher.Publish(key, PipelineEventType.InitialAdded, ctx, item);
            }

            foreach (MalformedObject malformedObject in page.Malformed)
            {
                await PublishMalformed(malformedObject, ctx);
            }

            ResetFailures();

            continueToken = page.ContinueToken;
            resourceVersion = page.ResourceVersion;

        } while (!string.IsNullOrEmpty(continueToken) && !ctx.IsCancellationRequested);

        if (!ctx.IsCancellationRequested)
        {
            initialListIncomplete = false;
        }

        return resourceVersion ?? string.Empty;
    }

    // in the middle of the list (continue token set), a failure worth retrying in place retries the same page,
    // so the pages already published stay valid; anything else goes to the main loop
    // the throttle slot is kept while waiting, which also keeps the pressure on the API server low
    private async Task<ResourcePage<TKubernetesObject>> GetInitialResourcesPage(
        string? continueToken,
        CancellationToken ctx)
    {
        while (true)
        {
            try
            {
                return await resourceWatch.GetInitialResources(
                    key,
                    continueToken,
                    options.Value.ResourceListPageSize,
                    ctx
                );
            }
            catch (Exception ex) when (!string.IsNullOrEmpty(continueToken) && !ctx.IsCancellationRequested)
            {
                WatchFailure failure = ClassifyNext(ex, WatchPhase.InitialList);

                if (!IsRetryableInPlace(failure.Category))
                {
                    throw;
                }

                CommitFailure(failure);

                await eventPublisher.Publish(key, PipelineEventType.Error, ctx, failure: failure);

                if (!await DelayRetry(failure, ctx))
                {
                    ctx.ThrowIfCancellationRequested();
                }
            }
        }
    }

    private async Task PublishWatchEvent(WatchEvent<TKubernetesObject> watchEvent, CancellationToken ctx)
    {
        if (watchEvent.Malformed is { } malformedObject)
        {
            await PublishMalformed(malformedObject, ctx);
            return;
        }

        await eventPublisher.Publish(key, watchEvent.Type.ToWatcherEvent(), ctx, watchEvent.Object);
    }

    private async Task PublishMalformed(MalformedObject malformedObject, CancellationToken ctx)
    {
        string malformedKey = $"{malformedObject.Uid}/{malformedObject.ResourceVersion}";

        LogLevel logLevel = loggedMalformedObjects.Add(malformedKey) ? LogLevel.Warning : LogLevel.Debug;

        logger.Log(
            logLevel,
            malformedObject.Exception,
            "Skipped malformed object - {kubernetesObjectType} - {key} - {namespace}/{name} - uid {uid} - resourceVersion {resourceVersion}",
            typeof(TKubernetesObject).Name,
            key,
            malformedObject.Namespace,
            malformedObject.Name,
            malformedObject.Uid,
            malformedObject.ResourceVersion
        );

        await eventPublisher.Publish(key, PipelineEventType.Skipped, ctx, malformed: malformedObject);
    }

    // waits before the next attempt; false when the session is stopping
    private async Task<bool> DelayRetry(WatchFailure failure, CancellationToken ctx)
    {
        TimeSpan retryDelay = retryPolicy.GetDelay(failure);

        LogLevel logLevel = failure.Category == WatchFailureCategory.Gone || !retryPolicy.IsRed(failure)
            ? LogLevel.Warning
            : LogLevel.Error;

        logger.Log(
            logLevel,
            failure.Exception,
            "Watcher {kubernetesObjectType} - {key} failed - {category} ({statusCode}) in {phase} - failure {consecutiveFailures} since {failingSince} - retry in {retryDelay}",
            typeof(TKubernetesObject).Name,
            key,
            failure.Category,
            failure.StatusCode,
            failure.Phase,
            failure.ConsecutiveFailures,
            failure.FailingSince,
            retryDelay
        );

        try
        {
            await Task.Delay(retryDelay, ctx);
            return true;
        }
        catch (OperationCanceledException) when (ctx.IsCancellationRequested)
        {
            return false;
        }
    }

    private WatchFailure ClassifyNext(Exception ex, WatchPhase phase) =>
        failureClassifier.Classify(
            ex,
            phase,
            consecutiveFailures + 1,
            consecutiveFailures == 0 ? DateTime.UtcNow : failingSince
        );

    private void CommitFailure(WatchFailure failure)
    {
        consecutiveFailures = failure.ConsecutiveFailures;
        failingSince = failure.FailingSince;
    }

    private void ResetFailures()
    {
        consecutiveFailures = 0;
    }

    private static bool IsRetryableInPlace(WatchFailureCategory category) => category is
        WatchFailureCategory.Throttled
        or WatchFailureCategory.ServerTransient
        or WatchFailureCategory.Transport
        or WatchFailureCategory.Unauthorized;

    private static string? GetResourceVersion(WatchEvent<TKubernetesObject> watchEvent)
    {
        string? resourceVersion = watchEvent.Object?.Metadata?.ResourceVersion ?? watchEvent.Malformed?.ResourceVersion;

        return string.IsNullOrEmpty(resourceVersion) ? null : resourceVersion;
    }

    private int GetWatcherRandomTimeout()
    {
        int configuredTimeout = options.Value.WatchTimeoutInSeconds;

        return Random.Shared.Next(configuredTimeout, (int)(configuredTimeout * 1.1));
    }

    public void Dispose()
    {
        cancellationTokenSource.Dispose();
    }
}
