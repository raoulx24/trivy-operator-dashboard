using k8s;
using k8s.Autorest;
using k8s.Models;
using Microsoft.Extensions.Options;
using System.Collections.Concurrent;
using System.Net;
using TrivyOperator.Dashboard.Application.K8sEventPipeline.Models.WatcherEvents;
using TrivyOperator.Dashboard.Application.K8sEventPipeline.Services.BackgroundQueues.Abstractions;
using TrivyOperator.Dashboard.Application.K8sEventPipeline.Services.Options;
using TrivyOperator.Dashboard.Application.Utils;
using TrivyOperator.Dashboard.Domain.K8s.ValueObjects;
using TrivyOperator.Dashboard.Infrastructure.Clients.Metrics.Abstractions;

namespace TrivyOperator.Dashboard.Application.K8sEventPipeline.Services.Watchers.Abstractions;

public abstract class KubernetesWatcher<TKubernetesObjectList, TKubernetesObject>(
    IKubernetesBackgroundQueue<TKubernetesObject> backgroundQueue,
    IOptions<WatchersOptions> options,
    IMetricsClient metricsClient,
    ILogger<KubernetesWatcher<TKubernetesObjectList, TKubernetesObject>> logger
) : IKubernetesWatcher
    where TKubernetesObject : class, IKubernetesObject<V1ObjectMeta>, new()
    where TKubernetesObjectList : IKubernetesObject<V1ListMeta>, IItems<TKubernetesObject>
{
    private static readonly Random random = new();
    private readonly double maxBackoffSeconds = 60;
    protected readonly int ResourceListPageSize = 500;
    protected readonly ConcurrentDictionary<WatcherKey, TaskWithCts> Watchers = [];
    
    public void StartWatcher(WatcherKey key, CancellationToken ctx = default)
    {
        if (Watchers.TryGetValue(key, out _))
        {
            logger.LogWarning(
                "Watcher for {kubernetesObjectType} and key {key} already existing. Ignoring Add req.",
                nameof(TKubernetesObject),
                key
            );
            return;
        }

        logger.LogInformation(
            "Adding Watcher for {kubernetesObjectType} and key {key}.",
            typeof(TKubernetesObject).Name,
            key
        );
        CancellationTokenSource cts = new();
        CancellationTokenSource linkedCts =
            CancellationTokenSource.CreateLinkedTokenSource(ctx, cts.Token);
        TaskWithCts watcherWithCts = new(CreateWatch(key, linkedCts.Token), cts, linkedCts);

        if (!Watchers.TryAdd(key, watcherWithCts))
        {
            logger.LogWarning(
                "Watcher for {kubernetesObjectType} and key {key} already exists. Ignoring Add req.",
                typeof(TKubernetesObject).Name,
                key
            );
            linkedCts.Cancel();
            watcherWithCts.Dispose();
        }
    }

    public async Task RecreateWatcher(WatcherKey key, CancellationToken ctx = default)
    {
        logger.LogWarning(
            "Recreated called for {kubernetesObjectType} - {key}",
            typeof(TKubernetesObject).Name,
            key
        );
        await StopWatcher(key, ctx);
        StartWatcher(key, ctx);
    }

    public async Task StopWatcher(WatcherKey key, CancellationToken ctx = default)
    {
        logger.LogInformation(
            "Deleting Watcher for {kubernetesObjectType} and key {key}.",
            typeof(TKubernetesObject).Name,
            key
        );
        await EnqueueWatcherEvent(key, WatcherEventType.Flushed, ctx);
        if (Watchers.TryGetValue(key, out TaskWithCts? taskWithCts))
        {
            await taskWithCts.Cts.CancelAsync();
            try
            {
                await taskWithCts.Task;
            }
            catch (TaskCanceledException)
            {
                // Task was canceled, ignore
            }
            catch (Exception ex)
            {
                logger.LogWarning(
                    ex,
                    "Watcher for {kubernetesObjectType} and key {key} crashed on Cts.Cancel() - {exceptionMessage}",
                    typeof(TKubernetesObject).Name,
                    key,
                    ex.Message
                );
            }
            finally
            {
                Watchers.TryRemove(key, out _);
                taskWithCts.Dispose();
            }
        }
        else
        {
            logger.LogWarning(
                "Watcher for {kubernetesObjectType} and key {key} not found. Ignoring Delete req.",
                typeof(TKubernetesObject).Name,
                key
            );
        }
    }

    public Type WatchedKubernetesObjectType => typeof(TKubernetesObject);

    private async Task CreateWatch(WatcherKey key, CancellationToken ctx = default)
    {
        string? lastResourceVersion = null;
        RetryDurationCalculator retryDurationCalculator = new(maxBackoffSeconds);
        int retryCount = -1; // -1 - first execution, 0 - no errors, 1, 2, 3 - retries

        while (!ctx.IsCancellationRequested)
        {
            bool shouldWaitForRetry = true;
            try
            {
#if DEBUG
                if (key.NamespaceName.Value == "watcher-test")
                {
                    HttpResponseMessage httpResponse = new(HttpStatusCode.Forbidden)
                    {
                        ReasonPhrase = "Access denied for test watcher",
                    };

                    throw new HttpOperationException
                    {
                        Response = new HttpResponseMessageWrapper(httpResponse, string.Empty),
                    };
                }
#endif
                if (string.IsNullOrEmpty(lastResourceVersion))
                {
                    lastResourceVersion = await ProcessInitialResourcesAndGetLastResourceVersion(
                        key,
                        ctx
                    );
                    logger.LogInformation(
                        "Initial Resources Processed - {kubernetesObjectType} - {key} - {lastResourceVersion}",
                        typeof(TKubernetesObject).Name,
                        key,
                        lastResourceVersion
                    );
                    await EnqueueWatcherEvent(key, WatcherEventType.Initialized, ctx);
                }

                do
                {
                    IAsyncEnumerable<WatchEvent<TKubernetesObject>> kubernetesObjectWatchList =
                        GetKubernetesObjectWatchList(key, lastResourceVersion, ctx);
                    await foreach (WatchEvent<TKubernetesObject> watchEvent in kubernetesObjectWatchList)
                    {
                        IncrementMetric(key, watchEvent.Type);

                        if (watchEvent.Type == WatchEventType.Bookmark)
                        {
                            lastResourceVersion = watchEvent.Object.Metadata.ResourceVersion;
                        }

                        logger.LogDebug(
                            "Sending to Queue - {kubernetesObjectType} - {kubernetesWatchEvent} - {key} - {kubernetesObjectName} - {kubernetesObjectResourceVersion}",
                            typeof(TKubernetesObject).Name,
                            watchEvent.Type.ToString(),
                            key,
                            watchEvent.Object.Metadata.Name,
                            watchEvent.Object.Metadata.ResourceVersion
                        );
                        await EnqueueWatcherEvent(key, watchEvent.Type.ToWatcherEvent(), ctx, watchEvent.Object);
                        retryCount = 0;
                    }

                    logger.LogDebug(
                        "Watch stopped - {kubernetesObjectType} - {key}",
                        typeof(TKubernetesObject).Name,
                        key
                    );
                } while (!ctx.IsCancellationRequested && !string.IsNullOrEmpty(lastResourceVersion));
            }
            catch (HttpRequestException ex) when (ex.InnerException is EndOfStreamException)
            {
                logger.LogDebug(
                    "Watcher {kubernetesObjectType} - {key} crashed - EndOfStreamException - {exceptionMessage}",
                    typeof(TKubernetesObject).Name,
                    key,
                    ex.Message
                );
            }
            catch (OperationCanceledException)
            {
                // be free and be gone :-)
            }
            catch (KubernetesException ex) when (ex.Message.StartsWith("too old resource version"))
            {
                logger.LogWarning(
                    "{kubernetesObjectType} - {key} - lastResourceVersion set to null - Too old resource version",
                    typeof(TKubernetesObject).Name,
                    key
                );
                shouldWaitForRetry = false;
            }
            catch (Exception ex)
            {
                await EnqueueWatcherEvent(key, WatcherEventType.Error, ctx, exception: ex);
                lastResourceVersion = null;
                logger.LogError(
                    ex,
                    "Watcher {kubernetesObjectType} - {key} crashed - {exceptionMessage}",
                    typeof(TKubernetesObject).Name,
                    key,
                    ex.Message
                );
            }

            if (ctx.IsCancellationRequested || !shouldWaitForRetry)
            {
                continue;
            }

            TimeSpan waitTimeSpan = retryDurationCalculator.GetNextRetryDuration(++retryCount);

            logger.LogDebug(
                "Watcher for {kubernetesObjectType} and key {key} is waiting for {retryCount} (ss:ms)",
                typeof(TKubernetesObject).Name,
                key,
                waitTimeSpan.ToString(@"ss\:fff")
            );
            await Task.Delay(waitTimeSpan, ctx);
        }
    }

    private async Task<string> ProcessInitialResourcesAndGetLastResourceVersion(
        WatcherKey key, CancellationToken ctx = default)
    {
        string? continueToken = null;
        string? lastResourceVersion;

        do
        {
            TKubernetesObjectList customResourceList = await GetInitialResources(
                key,
                continueToken,
                ctx
            );

            foreach (TKubernetesObject item in customResourceList.Items ?? [])
            {
                await EnqueueWatcherEvent(key, WatcherEventType.InitialAdded, ctx, item);
            }

            continueToken = customResourceList.Metadata.ContinueProperty;
            lastResourceVersion = customResourceList.Metadata.ResourceVersion;
        } while (!string.IsNullOrEmpty(continueToken) && !ctx.IsCancellationRequested);

        return lastResourceVersion;
    }

    protected abstract Task<TKubernetesObjectList> GetInitialResources(
        WatcherKey key,
        string? continueToken,
        CancellationToken ctx = default
    );

    protected abstract IAsyncEnumerable<WatchEvent<TKubernetesObject>> GetKubernetesObjectWatchList(
        WatcherKey key,
        string? lastResourceVersion,
        CancellationToken cancellationToken = default
    );

    protected virtual void ProcessReceivedKubernetesObject(TKubernetesObject kubernetesObject)
    { }

    protected int GetWatcherRandomTimeout() => random.Next(
        options.Value.WatchTimeoutInSeconds,
        (int)(options.Value.WatchTimeoutInSeconds * 1.1)
    );

    private void IncrementMetric(WatcherKey key, WatchEventType watchEventType, int value = 1) =>
        metricsClient.WatcherProcessedMessagesCounter.Add(
            value,
            new KeyValuePair<string, object?>("resource_kind", typeof(TKubernetesObject).Name),
            new KeyValuePair<string, object?>(
                "resource_level",
                key.NamespaceName.IsClusterScoped ? "cluster_scoped" : "namespaced"
            ),
            new KeyValuePair<string, object?>(
                "context_name",
                key.ContextName.IsUnset ? null : key.ContextName.Value
            ),
            new KeyValuePair<string, object?>(
                "namespace_name",
                key.NamespaceName.IsClusterScoped ? null : key.NamespaceName.Value
            ),
            new KeyValuePair<string, object?>("watch_event_type", watchEventType.ToString())
        );

    private async Task EnqueueWatcherEvent(
        WatcherKey key,
        WatcherEventType watchEventType,
        CancellationToken cancellationToken,
        TKubernetesObject? kubernetesObject = null,
        Exception? exception = null
    )
    {
        logger.LogDebug(
            "Sending to Queue - {kubernetesObjectType} - {kubernetesWatchEvent} - {key} - {kubernetesObjectName}",
            typeof(TKubernetesObject).Name,
            watchEventType.ToString(),
            key,
            kubernetesObject?.Metadata?.Name ?? "N/A"
        );
        try
        {
            if (kubernetesObject != null)
            {
                ProcessReceivedKubernetesObject(kubernetesObject);
            }

            WatcherEvent<TKubernetesObject> kubernetesWatcherEvent = new()
            {
                Key = key,
                KubernetesObject = kubernetesObject,
                WatcherEventType = watchEventType,
                Exception = exception,
            };
            await backgroundQueue.QueueBackgroundWorkItemAsync(kubernetesWatcherEvent, cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogError(
                ex,
                "Watcher for {kubernetesObjectType} and key {key} could not enqueue {kubernetesWatchEvent} - {exceptionMessage}",
                typeof(TKubernetesObject).Name,
                key,
                watchEventType.ToString(),
                ex.Message
            );
        }
    }
}
