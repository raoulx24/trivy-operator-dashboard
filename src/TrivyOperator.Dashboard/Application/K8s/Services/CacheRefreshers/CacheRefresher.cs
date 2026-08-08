using k8s;
using k8s.Models;
using System.Collections.Concurrent;
using TrivyOperator.Dashboard.Application.K8s.Models.WatcherEvents;
using TrivyOperator.Dashboard.Application.K8s.Pipeline;
using TrivyOperator.Dashboard.Infrastructure.Caching.InMemoryOld.Abstractions;
using TrivyOperator.Dashboard.Infrastructure.Utils;

namespace TrivyOperator.Dashboard.Application.K8s.Services.CacheRefreshers;

public class CacheRefresher<TKubernetesObject>(
    IConcurrentDictionaryCache<TKubernetesObject> cache,
    ILogger<CacheRefresher<TKubernetesObject>> logger
) : IKubernetesEventProcessor<TKubernetesObject>
    where TKubernetesObject : IKubernetesObject<V1ObjectMeta>, new()
{
    protected readonly IConcurrentDictionaryCache<TKubernetesObject> Cache = cache;

    public async Task ProcessKubernetesEvent(WatcherEvent<TKubernetesObject> watcherEvent, CancellationToken ctx = default)
    {
        ctx.ThrowIfCancellationRequested();
        
        switch (watcherEvent.WatcherEventType)
        {
            case WatcherEventType.InitialAdded:
            case WatcherEventType.Added:
                ProcessAddEvent(watcherEvent, ctx);
                break;
            case WatcherEventType.Deleted:
                await ProcessDeleteEvent(watcherEvent, ctx);
                break;
            case WatcherEventType.Error:
            case WatcherEventType.Flushed:
                ProcessErrorEvent(watcherEvent);
                break;
            case WatcherEventType.Modified:
                ProcessModifiedEvent(watcherEvent, ctx);
                break;
            case WatcherEventType.Initialized:
                await ProcessInitEvent(watcherEvent, ctx);
                break;
            case WatcherEventType.Unknown:
                logger.LogWarning(
                    "Unknown event type {eventType} for {kubernetesObjectType}.",
                    watcherEvent.WatcherEventType,
                    typeof(TKubernetesObject).Name
                );
                break;
        }
    }

    protected virtual void ProcessAddEvent(WatcherEvent<TKubernetesObject> watcherEvent, CancellationToken ctx = default)
    {
        if (watcherEvent.KubernetesObject == null)
        {
            logger.LogWarning(
                "ProcessAddEvent - KubernetesObject is null for {key} {kubernetesObjectType}. Ignoring",
                watcherEvent.Key,
                typeof(TKubernetesObject).Name
            );
            return;
        }

        logger.LogDebug(
            "ProcessAddEvent - {kubernetesObjectType} - {key} - {kubernetesObjectName}",
            typeof(TKubernetesObject).Name,
            watcherEvent.Key,
            watcherEvent.KubernetesObject.Metadata.Name
        );

        if (Cache.TryGetValue(
                watcherEvent.Key.NamespaceName.Value,
                out ConcurrentDictionary<string, TKubernetesObject>? kubernetesObjectsCache
            ))
        {
            kubernetesObjectsCache[watcherEvent.KubernetesObject.Uid()] = watcherEvent.KubernetesObject;
        }
        else // first time, the cache is really empty
        {
            Cache.TryAdd(
                watcherEvent.Key.NamespaceName.Value,
                new ConcurrentDictionary<string, TKubernetesObject>
                {
                    [watcherEvent.KubernetesObject.Uid()] = watcherEvent.KubernetesObject,
                }
            );
        }
    }

    protected virtual Task ProcessDeleteEvent(
        WatcherEvent<TKubernetesObject> watcherEvent,
        CancellationToken ctx = default
    )
    {
        if (watcherEvent.KubernetesObject == null)
        {
            logger.LogWarning(
                "ProcessDeleteEvent - KubernetesObject is null for {Key} - {kubernetesObjectType}. Ignoring",
                watcherEvent.Key,
                typeof(TKubernetesObject).Name
            );
            return Task.CompletedTask;
        }

        logger.LogDebug(
            "ProcessDeleteEvent - {kubernetesObjectType} - {key} - {kubernetesObjectName}",
            typeof(TKubernetesObject).Name,
            watcherEvent.Key,
            watcherEvent.KubernetesObject.Metadata.Name
        );

        if (!Cache.TryGetValue(
                watcherEvent.Key.NamespaceName.Value,
                out ConcurrentDictionary<string, TKubernetesObject>? kubernetesObjectsCache
            ))
        {
            return Task.CompletedTask;
        }

        kubernetesObjectsCache.TryRemove(watcherEvent.KubernetesObject.Uid(), out _);

        return Task.CompletedTask;
    }

    protected virtual void ProcessErrorEvent(WatcherEvent<TKubernetesObject> watcherEvent)
    {
        string watcherKey = watcherEvent.Key.NamespaceName.Value;
        logger.LogDebug(
            "ProcessErrorEvent - {kubernetesObjectType} - {key}",
            typeof(TKubernetesObject).Name,
            watcherEvent.Key
        );
        Cache.TryRemove(watcherKey, out _);
    }

    protected virtual void ProcessModifiedEvent(
        WatcherEvent<TKubernetesObject> watcherEvent,
        CancellationToken ctx = default
    )
    {
        logger.LogDebug("ProcessModifiedEvent - redirecting to ProcessAddEvent.");
        ProcessAddEvent(watcherEvent, ctx);
    }

    protected virtual Task ProcessInitEvent(
        WatcherEvent<TKubernetesObject> watcherEvent,
        CancellationToken ctx = default
    ) => Task.CompletedTask;
}
