using k8s;
using k8s.Models;
using System.Collections.Concurrent;
using TrivyOperator.Dashboard.Application.Services.KubernetesEventDispatchers.Abstractions;
using TrivyOperator.Dashboard.Application.Services.WatcherEvents;
using TrivyOperator.Dashboard.Application.Services.WatcherEvents.Abstractions;
using TrivyOperator.Dashboard.Infrastructure.Abstractions;
using TrivyOperator.Dashboard.Utils;

namespace TrivyOperator.Dashboard.Application.Services.CacheRefreshers;

public class CacheRefresher<TKubernetesObject>(
    IConcurrentDictionaryCache<TKubernetesObject> cache,
    ILogger<CacheRefresher<TKubernetesObject>> logger)
    : IKubernetesEventProcessor<TKubernetesObject>
    where TKubernetesObject : IKubernetesObject<V1ObjectMeta>
{
    protected IConcurrentDictionaryCache<TKubernetesObject> cache = cache;

    public async Task ProcessKubernetesEvent(IWatcherEvent<TKubernetesObject> watcherEvent, CancellationToken cancellationToken)
    {
        switch (watcherEvent.WatcherEventType)
        {
            case WatcherEventType.InitialAdded:
            case WatcherEventType.Added:
                ProcessAddEvent(watcherEvent, cancellationToken);
                break;
            case WatcherEventType.Deleted:
                await ProcessDeleteEvent(watcherEvent, cancellationToken);
                break;
            case WatcherEventType.Error:
            case WatcherEventType.Flushed:
                ProcessErrorEvent(watcherEvent);
                break;
            case WatcherEventType.Modified:
                ProcessModifiedEvent(watcherEvent, cancellationToken);
                break;
            case WatcherEventType.Initialized:
                await ProcessInitEvent(watcherEvent, cancellationToken);
                break;
            case WatcherEventType.Unknown:
                logger.LogWarning("Unknown event type {eventType} for {kubernetesObjectType}.",
                    watcherEvent.WatcherEventType, typeof(TKubernetesObject).Name);
                break;
            default:
                break;
        }
    }

    protected virtual void ProcessAddEvent(
        IWatcherEvent<TKubernetesObject> watcherEvent,
        CancellationToken cancellationToken)
    {
        if (watcherEvent.KubernetesObject == null)
        {
            logger.LogWarning("ProcessAddEvent - KubernetesObject is null for {watcherKey} {kubernetesObjectType}. Ignoring", 
                watcherEvent.WatcherKey, typeof(TKubernetesObject).Name);
            return;
        }

        logger.LogDebug(
            "ProcessAddEvent - {kubernetesObjectType} - {watcherKey} - {kubernetesObjectName}",
            typeof(TKubernetesObject).Name,
            watcherEvent.WatcherKey,
            watcherEvent.KubernetesObject.Metadata.Name);

        if (cache.TryGetValue(watcherEvent.WatcherKey, out ConcurrentDictionary<string, TKubernetesObject>? kubernetesObjectsCache))
        {
            kubernetesObjectsCache[watcherEvent.KubernetesObject.Uid()] = watcherEvent.KubernetesObject;
        }
        else // first time, the cache is really empty
        {
            cache.TryAdd(watcherEvent.WatcherKey, new ConcurrentDictionary<string, TKubernetesObject> { 
                [watcherEvent.KubernetesObject.Uid()] = watcherEvent.KubernetesObject 
            });
        }
    }

    protected virtual Task ProcessDeleteEvent(IWatcherEvent<TKubernetesObject> watcherEvent, CancellationToken cancellationToken)
    {
        if (watcherEvent.KubernetesObject == null)
        {
            logger.LogWarning("ProcessDeleteEvent - KubernetesObject is null for {watcherKey} {kubernetesObjectType}. Ignoring", 
                watcherEvent.WatcherKey, typeof(TKubernetesObject).Name);
            return Task.CompletedTask;
        }

        logger.LogDebug(
            "ProcessDeleteEvent - {kubernetesObjectType} - {watcherKey} - {kubernetesObjectName}",
            typeof(TKubernetesObject).Name,
            watcherEvent.WatcherKey,
            watcherEvent.KubernetesObject.Metadata.Name);

        if (!cache.TryGetValue(watcherEvent.WatcherKey, out ConcurrentDictionary<string, TKubernetesObject>? kubernetesObjectsCache))
        {
            return Task.CompletedTask;
        }

        kubernetesObjectsCache.TryRemove(watcherEvent.KubernetesObject.Uid(), out _);

        return Task.CompletedTask;
    }

    protected virtual void ProcessErrorEvent(IWatcherEvent<TKubernetesObject> watcherEvent)
    {
        string watcherKey = CacheUtils.GetCacheRefreshKey(watcherEvent.KubernetesObject);
        logger.LogDebug(
            "ProcessErrorEvent - {kubernetesObjectType} - {watcherKey}",
            typeof(TKubernetesObject).Name,
            watcherKey);
        cache.TryRemove(watcherKey, out _);
    }

    protected virtual void ProcessModifiedEvent(
        IWatcherEvent<TKubernetesObject> watcherEvent,
        CancellationToken cancellationToken)
    {
        logger.LogDebug("ProcessModifiedEvent - redirecting to ProcessAddEvent.");
        ProcessAddEvent(watcherEvent, cancellationToken);
    }

    protected virtual Task ProcessInitEvent(IWatcherEvent<TKubernetesObject> watcherEvent, CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}
