using k8s;
using k8s.Models;
using Microsoft.Extensions.Options;
using TrivyOperator.Dashboard.Application.Kubernetes.WatcherState.Options;
using TrivyOperator.Dashboard.Domain.Kubernetes.ValueObjects;
using TrivyOperator.Dashboard.Infrastructure.Kubernetes.EventPipeline.Models;
using TrivyOperator.Dashboard.Infrastructure.Kubernetes.EventPipeline.Services.EventPublishers.Abstractions;
using TrivyOperator.Dashboard.Infrastructure.Kubernetes.EventPipeline.Services.ResourceWatches.Abstractions;

namespace TrivyOperator.Dashboard.Infrastructure.Kubernetes.EventPipeline.Services.WatchSessions;

public sealed class KubernetesWatchSession<TKubernetesObjectList, TKubernetesObject>(
    ResourceLocation key,
    IKubernetesResourceWatch<TKubernetesObjectList, TKubernetesObject> resourceWatch,
    IKubernetesEventPublisher<TKubernetesObject> eventPublisher,
    IOptions<WatchersOptions> options,
    ILogger<KubernetesWatchSession<TKubernetesObjectList, TKubernetesObject>> logger
) : IDisposable
    where TKubernetesObject : class, IKubernetesObject<V1ObjectMeta>, new()
    where TKubernetesObjectList : IKubernetesObject<V1ListMeta>, IItems<TKubernetesObject>
{
    private const int ResourceListPageSize = 500;
    private const double MaxBackoffSeconds = 60;

    private static readonly Random Random = new();

    private readonly CancellationTokenSource cancellationTokenSource = new();
    private CancellationToken linkedCancellationToken;

    private Task? runningTask;

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

        linkedCancellationToken =
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
    }

    private async Task Run(CancellationToken ctx = default)
    {
        string? resourceVersion = null;

        int retryCount = -1;

        while (!ctx.IsCancellationRequested)
        {
            bool shouldRetry = true;

            try
            {
                if (string.IsNullOrEmpty(resourceVersion))
                {
                    resourceVersion = await ProcessInitialResources(ctx);

                    logger.LogInformation(
                        "Initial Resources Processed - {kubernetesObjectType} - {key} - {resourceVersion}",
                        typeof(TKubernetesObject).Name,
                        key,
                        resourceVersion
                    );

                    await eventPublisher.Publish(key, WatcherEventType.Initialized, ctx);
                }

                do
                {
                    IAsyncEnumerable<WatchEvent<TKubernetesObject>> watchList =
                        resourceWatch.GetWatchList(key, resourceVersion, GetWatcherRandomTimeout(), ctx);

                    await foreach (WatchEvent<TKubernetesObject> watchEvent in watchList)
                    {
                        if (watchEvent.Type == WatchEventType.Bookmark)
                        {
                            resourceVersion = watchEvent.Object.Metadata.ResourceVersion;
                        }

                        await eventPublisher.Publish(key, watchEvent.Type.ToWatcherEvent(), ctx, watchEvent.Object);

                        retryCount = 0;
                    }

                    logger.LogDebug(
                        "Watch stopped - {kubernetesObjectType} - {key}",
                        typeof(TKubernetesObject).Name,
                        key
                    );

                } while (!ctx.IsCancellationRequested && !string.IsNullOrEmpty(resourceVersion));
            }
            catch (HttpRequestException ex) when (ex.InnerException is EndOfStreamException)
            {
                logger.LogDebug(
                    ex,
                    "Watcher {kubernetesObjectType} - {key} crashed - EndOfStreamException",
                    typeof(TKubernetesObject).Name,
                    key
                );
            }
            catch (OperationCanceledException)
            {
                // Session is stopping.
            }
            catch (KubernetesException ex) when (ex.Message.StartsWith("too old resource version"))
            {
                logger.LogWarning(
                    "{kubernetesObjectType} - {key} - resetting resourceVersion because it is too old",
                    typeof(TKubernetesObject).Name,
                    key
                );

                shouldRetry = false;
                resourceVersion = null;
            }
            catch (Exception ex)
            {
                await eventPublisher.Publish(key, WatcherEventType.Error, ctx, exception: ex);

                resourceVersion = null;

                logger.LogError(
                    ex,
                    "Watcher {kubernetesObjectType} - {key} crashed - {exceptionMessage}",
                    typeof(TKubernetesObject).Name,
                    key,
                    ex.Message
                );
            }

            if (ctx.IsCancellationRequested || !shouldRetry)
            {
                continue;
            }

            TimeSpan retryDelay = GetNextRetryDuration(++retryCount, MaxBackoffSeconds);

            logger.LogDebug(
                "Watcher for {kubernetesObjectType} and key {key} waiting for retry {retryCount} ({retryDelay})",
                typeof(TKubernetesObject).Name,
                key,
                retryCount,
                retryDelay
            );

            await Task.Delay(retryDelay, ctx);
        }
    }

    private async Task<string> ProcessInitialResources(CancellationToken ctx = default)
    {
        string? continueToken = null;
        string? resourceVersion = null;

        do
        {
            TKubernetesObjectList resourceList =
                await resourceWatch.GetInitialResources(
                    key,
                    continueToken,
                    ResourceListPageSize,
                    ctx
                );

            foreach (TKubernetesObject item in resourceList.Items ?? [])
            {
                await eventPublisher.Publish(key, WatcherEventType.InitialAdded, ctx, item);
            }

            continueToken = resourceList.Metadata.ContinueProperty;

            resourceVersion = resourceList.Metadata.ResourceVersion;

        } while (!string.IsNullOrEmpty(continueToken) && !ctx.IsCancellationRequested);

        return resourceVersion ?? string.Empty;
    }

    private int GetWatcherRandomTimeout()
    {
        int configuredTimeout = options.Value.WatchTimeoutInSeconds;

        return Random.Next(configuredTimeout, (int)(configuredTimeout * 1.1));
    }
    
    private static TimeSpan GetNextRetryDuration(int retryAttempt, double maxBackoffSeconds) =>
        TimeSpan.FromSeconds(maxBackoffSeconds * Math.Log(retryAttempt + 1));

    public void Dispose()
    {
        cancellationTokenSource.Dispose();
    }
}
