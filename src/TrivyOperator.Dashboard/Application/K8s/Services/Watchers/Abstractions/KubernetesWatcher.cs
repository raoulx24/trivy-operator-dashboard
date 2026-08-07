using k8s;
using k8s.Autorest;
using k8s.Models;
using Microsoft.Extensions.Options;
using System.Collections.Concurrent;
using System.Net;
using TrivyOperator.Dashboard.Application.K8s.Pipeline;
using TrivyOperator.Dashboard.Application.K8s.Services.Options;
using TrivyOperator.Dashboard.Application.K8s.Services.WatcherEvents;
using TrivyOperator.Dashboard.Application.Utils;
using TrivyOperator.Dashboard.Domain.K8s.ValueObjects;
using TrivyOperator.Dashboard.Infrastructure.Clients.Abstractions;
using TrivyOperator.Dashboard.Infrastructure.K8s.Services.Abstractions;
using TrivyOperator.Dashboard.Infrastructure.Utils;

namespace TrivyOperator.Dashboard.Application.K8s.Services.Watchers.Abstractions;

public abstract class
    KubernetesWatcher<TKubernetesObjectList, TKubernetesObject, TBackgroundQueue, TKubernetesWatcherEvent>(
        IKubernetesResourceService<TKubernetesObject> kubernetesservice,
        TBackgroundQueue backgroundQueue,
        IOptions<WatchersOptions> options,
        IMetricsClient metricsClient,
        ILogger<KubernetesWatcher<TKubernetesObjectList, TKubernetesObject, TBackgroundQueue, TKubernetesWatcherEvent>>
            logger
    ) : IKubernetesWatcher<TKubernetesObject>
    where TKubernetesObject : class, IKubernetesObject<V1ObjectMeta>, new()
    where TKubernetesObjectList : IKubernetesObject<V1ListMeta>, IItems<TKubernetesObject>
    where TKubernetesWatcherEvent : IWatcherEvent<TKubernetesObject>, new()
    where TBackgroundQueue : IKubernetesBackgroundQueue<TKubernetesObject>

{
    private static readonly Random random = new();
    protected readonly TBackgroundQueue BackgroundQueue = backgroundQueue;
    protected readonly double maxBackoffSeconds = 60;
    protected readonly int resourceListPageSize = 500;
    protected readonly ConcurrentDictionary<string, TaskWithCts> Watchers = [];
    
    // TODO: it's not ok here, it should come from above
    protected ContextName CurrentContextName = new();

    public Task Add(CancellationToken cancellationToken, string watcherKey = CacheUtils.DefaultCacheRefreshKey)
    {
        watcherKey = string.IsNullOrWhiteSpace(watcherKey) ? CacheUtils.DefaultCacheRefreshKey : watcherKey;
        CurrentContextName = kubernetesservice.GetCurrentContext();

        if (Watchers.TryGetValue(watcherKey, out _))
        {
            logger.LogWarning(
                "Watcher for {kubernetesObjectType} and key {watcherKey} already existing. Ignoring Add req.",
                typeof(TKubernetesObject).Name,
                watcherKey
            );
            return Task.CompletedTask;
        }

        logger.LogInformation(
            "Adding Watcher for {kubernetesObjectType} and key {watcherKey}.",
            typeof(TKubernetesObject).Name,
            watcherKey
        );
        CancellationTokenSource cts = new();
        CancellationTokenSource linkedCts =
            CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, cts.Token);
        TaskWithCts watcherWithCts = new(CreateWatch(watcherKey, linkedCts.Token), cts, linkedCts);

        if (!Watchers.TryAdd(watcherKey, watcherWithCts))
        {
            logger.LogWarning(
                "Watcher for {kubernetesObjectType} and key {watcherKey} already exists. Ignoring Add req.",
                typeof(TKubernetesObject).Name,
                watcherKey
            );
            return Task.CompletedTask;
        }

        return Task.CompletedTask;
    }

    public async Task Recreate(
        CancellationToken cancellationToken,
        string watcherKey = CacheUtils.DefaultCacheRefreshKey
    )
    {
        watcherKey = string.IsNullOrWhiteSpace(watcherKey) ? CacheUtils.DefaultCacheRefreshKey : watcherKey;

        logger.LogWarning(
            "Recreated called for {kubernetesObjectType} - {watcherKey}",
            typeof(TKubernetesObject).Name,
            watcherKey
        );
        await Delete(watcherKey, cancellationToken);
        await Add(cancellationToken, watcherKey);
    }

    public async Task Delete(string watcherKey, CancellationToken cancellationToken)
    {
        watcherKey = string.IsNullOrWhiteSpace(watcherKey) ? CacheUtils.DefaultCacheRefreshKey : watcherKey;

        logger.LogInformation(
            "Deleting Watcher for {kubernetesObjectType} and key {watcherKey}.",
            typeof(TKubernetesObject).Name,
            watcherKey
        );
        await EnqueueWatcherEvent(watcherKey, WatcherEventType.Flushed, cancellationToken);
        if (Watchers.TryGetValue(watcherKey, out TaskWithCts? taskWithCts))
        {
            await taskWithCts.Cts.CancelAsync();
            try
            {
                await taskWithCts.Task;
            }
            catch (TaskCanceledException)
            {
                // Task was cancelled, ignore
            }
            catch (Exception ex)
            {
                logger.LogWarning(
                    ex,
                    "Watcher for {kubernetesObjectType} and key {watcherKey} crashed on Cts.Cancel() - {exceptionMessage}",
                    typeof(TKubernetesObject).Name,
                    watcherKey,
                    ex.Message
                );
            }
            finally
            {
                Watchers.TryRemove(watcherKey, out _);
                taskWithCts.Dispose();
            }
        }
        else
        {
            logger.LogWarning(
                "Watcher for {kubernetesObjectType} and key {watcherKey} not found. Ignoring Delete req.",
                typeof(TKubernetesObject).Name,
                watcherKey
            );
        }
    }

    protected async Task CreateWatch(string watcherKey, CancellationToken cancellationToken)
    {
        string? lastResourceVersion = null;
        RetryDurationCalculator retryDurationCalculator = new(maxBackoffSeconds);
        int retryCount = -1; // -1 - first execution, 0 - no errors, 1, 2, 3 - retries

        while (!cancellationToken.IsCancellationRequested)
        {
            bool shouldWaitForRetry = true;
            try
            {
#if DEBUG
                if (watcherKey == "watcher-test")
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
                        watcherKey,
                        cancellationToken
                    );
                    logger.LogInformation(
                        "Initial Resources Processed - {kubernetesObjectType} - {watcherKey} - {lastResourceVersion}",
                        typeof(TKubernetesObject).Name,
                        watcherKey,
                        lastResourceVersion
                    );
                    await EnqueueWatcherEvent(watcherKey, WatcherEventType.Initialized, cancellationToken);
                }

                do
                {
                    IAsyncEnumerable<WatchEvent<TKubernetesObject>> kubernetesObjectWatchList =
                        GetKubernetesObjectWatchList(watcherKey, lastResourceVersion, async ex =>
                        {
                            if (ex is KubernetesException &&
                                ex.Message.StartsWith("too old resource version"))
                            {
                                logger.LogWarning(
                                    "{kubernetesObjectType} - {watcherKey} - lastResourceVersion set to null - Too old resource version",
                                    typeof(TKubernetesObject).Name,
                                    watcherKey
                                );
                                shouldWaitForRetry = false;
                            }
                            else
                            {
                                logger.LogError(
                                    ex,
                                    "Watcher {kubernetesObjectType} - {watcherKey} crashed - {exceptionMessage}",
                                    typeof(TKubernetesObject).Name,
                                    watcherKey,
                                    ex.Message
                                );
                            }

                            lastResourceVersion = null;
                            await EnqueueWatcherEvent(
                                watcherKey,
                                WatcherEventType.Error,
                                cancellationToken
                            );
                        }, cancellationToken);
                    await foreach (WatchEvent<TKubernetesObject> watchEvent in kubernetesObjectWatchList)
                    {
                        IncrementMetric(watcherKey, watchEvent.Type);

                        if (watchEvent.Type == WatchEventType.Bookmark)
                        {
                            lastResourceVersion = watchEvent.Object.Metadata.ResourceVersion;
                        }

                        logger.LogDebug(
                            "Sending to Queue - {kubernetesObjectType} - {kubernetesWatchEvent} - {watcherKey} - {kubernetesObjectName} - {kubernetesObjectResourceVersion}",
                            typeof(TKubernetesObject).Name,
                            watchEvent.Type.ToString(),
                            watcherKey,
                            watchEvent.Object.Metadata.Name,
                            watchEvent.Object.Metadata.ResourceVersion
                        );
                        await EnqueueWatcherEvent(watcherKey, watchEvent.Type.ToWatcherEvent(), cancellationToken, watchEvent.Object);
                        retryCount = 0;
                    }

                    logger.LogDebug(
                        "Watch stopped - {kubernetesObjectType} - {watcherKey}",
                        typeof(TKubernetesObject).Name,
                        watcherKey
                    );
                } while (!cancellationToken.IsCancellationRequested && !string.IsNullOrEmpty(lastResourceVersion));
            }
            catch (HttpRequestException ex) when (ex.InnerException is EndOfStreamException)
            {
                logger.LogDebug(
                    "Watcher {kubernetesObjectType} - {watcherKey} crashed - EndOfStreamException - {exceptionMessage}",
                    typeof(TKubernetesObject).Name,
                    watcherKey,
                    ex.Message
                );
            }
            catch (OperationCanceledException)
            {
                // be free and be gone :-)
            }
            catch (Exception ex)
            {
                await EnqueueWatcherEvent(watcherKey, WatcherEventType.Error, cancellationToken, exception: ex);
                lastResourceVersion = null;
                logger.LogError(
                    ex,
                    "Watcher {kubernetesObjectType} - {watcherKey} crashed - {exceptionMessage}",
                    typeof(TKubernetesObject).Name,
                    watcherKey,
                    ex.Message
                );
            }

            if (cancellationToken.IsCancellationRequested || !shouldWaitForRetry)
            {
                continue;
            }

            TimeSpan waitTimeSpan = retryDurationCalculator.GetNextRetryDuration(++retryCount);

            logger.LogDebug(
                "Watcher for {kubernetesObjectType} and key {watcherKey} is wating for {retryCount} (ss:ms)",
                typeof(TKubernetesObject).Name,
                watcherKey,
                waitTimeSpan.ToString(@"ss\:fff")
            );
            await Task.Delay(waitTimeSpan, cancellationToken);
        }
    }

    protected async Task<string> ProcessInitialResourcesAndGetLastResourceVersion(
        string watcherKey,
        CancellationToken cancellationToken
    )
    {
        string? continueToken = null;
        string? lastResourceVersion;

        do
        {
            TKubernetesObjectList customResourceList = await GetInitialResources(
                watcherKey,
                continueToken,
                cancellationToken
            );

            foreach (TKubernetesObject item in customResourceList.Items ?? [])
            {
                await EnqueueWatcherEvent(watcherKey, WatcherEventType.InitialAdded, cancellationToken, item);
            }

            continueToken = customResourceList.Metadata.ContinueProperty;
            lastResourceVersion = customResourceList.Metadata.ResourceVersion;
        } while (!string.IsNullOrEmpty(continueToken) && !cancellationToken.IsCancellationRequested);

        return lastResourceVersion;
    }

    protected abstract Task<TKubernetesObjectList> GetInitialResources(
        string watcherKey,
        string? continueToken,
        CancellationToken cancellationToken = default
    );

    protected abstract IAsyncEnumerable<WatchEvent<TKubernetesObject>> GetKubernetesObjectWatchList(
        string watcherKey,
        string? lastResourceVersion,
        Action<Exception>? onError = null,
        CancellationToken cancellationToken = default
    );

    protected virtual void ProcessReceivedKubernetesObject(TKubernetesObject kubernetesObject)
    {
    }

    protected int GetWatcherRandomTimeout() => random.Next(
        options.Value.WatchTimeoutInSeconds,
        (int)(options.Value.WatchTimeoutInSeconds * 1.1)
    );

    protected void IncrementMetric(string watcherKey, WatchEventType watchEventType, int value = 1) =>
        metricsClient.WatcherProcessedMessagesCounter.Add(
            value,
            new KeyValuePair<string, object?>("resource_kind", typeof(TKubernetesObject).Name),
            new KeyValuePair<string, object?>(
                "resource_level",
                watcherKey == CacheUtils.DefaultCacheRefreshKey ? "cluster_scoped" : "namespaced"
            ),
            new KeyValuePair<string, object?>(
                "namespace_name",
                watcherKey == CacheUtils.DefaultCacheRefreshKey ? null : watcherKey
            ),
            new KeyValuePair<string, object?>("watch_event_type", watchEventType.ToString())
        );

    protected async Task EnqueueWatcherEvent(
        string watcherKey,
        WatcherEventType watchEventType,
        CancellationToken cancellationToken,
        TKubernetesObject? kubernetesObject = null,
        Exception? exception = null
    )
    {
        logger.LogDebug(
            "Sending to Queue - {kubernetesObjectType} - {kubernetesWatchEvent} - {watcherKey} - {kubernetesObjectName}",
            typeof(TKubernetesObject).Name,
            watchEventType.ToString(),
            watcherKey,
            kubernetesObject?.Metadata?.Name ?? "N/A"
        );
        try
        {
            if (kubernetesObject != null)
            {
                ProcessReceivedKubernetesObject(kubernetesObject);
            }

            TKubernetesWatcherEvent kubernetesWatcherEvent = new()
            {
                KubernetesObject = kubernetesObject,
                ContextName = CurrentContextName,
                WatcherEventType = watchEventType,
                WatcherKey = watcherKey,
                Exception = exception,
            };
            await BackgroundQueue.QueueBackgroundWorkItemAsync(kubernetesWatcherEvent, cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogError(
                ex,
                "Watcher for {kubernetesObjectType} and key {watcherKey} could not enqueue {kubernetesWatchEvent} - {exceptionMessage}",
                typeof(TKubernetesObject).Name,
                watcherKey,
                watchEventType.ToString(),
                ex.Message
            );
        }
    }
}
