using TrivyOperator.Dashboard.Application.Kubernetes.EventPipeline.EventProcessors.Abstractions;
using TrivyOperator.Dashboard.Application.Kubernetes.Models;
using TrivyOperator.Dashboard.Domain.Kubernetes.ValueObjects;
using TrivyOperator.Dashboard.Domain.Shared.Abstractions;
using TrivyOperator.Dashboard.Domain.Shared.Stores.Abstractions;

namespace TrivyOperator.Dashboard.Application.Kubernetes.EventPipeline.EventProcessors;

public class ResourceStoreUpdater<TResource, TKey> (
    IResourceStore<TResource, TKey> resourceStore,
    ILogger<ResourceStoreUpdater<TResource, TKey>> logger
) : IKubernetesEventProcessor<TResource, TKey>
    where TResource : class, IEntity<TKey>
{
    public async Task ProcessKubernetesEvent(
        WatcherEvent<TResource, TKey> watcherEvent,
        CancellationToken ctx
    )
    {
        switch (watcherEvent.WatcherEventType)
        {
            case WatcherEventType.InitialAdded:
            case WatcherEventType.Added:
            case WatcherEventType.Modified:
                await ProcessAddEvent(watcherEvent, ctx);
                break;
            case WatcherEventType.Deleted:
                await ProcessDeleteEvent(watcherEvent, ctx);
                break;
            case WatcherEventType.Error:
            case WatcherEventType.Flushed:
                await ProcessErrorEvent(watcherEvent, ctx);
                break;
            case WatcherEventType.Initialized:
                ProcessInitEvent(watcherEvent);
                break;
            case WatcherEventType.Bookmark:
            case WatcherEventType.WatcherConnected:
                break;
            case WatcherEventType.Unknown:
            default:
                logger.LogWarning(
                    "Unknown event type {eventType} for {kubernetesObjectType}.",
                    watcherEvent.WatcherEventType,
                    typeof(TResource).Name
                );
                break;
        }
    }
    
    private async Task ProcessAddEvent(WatcherEvent<TResource, TKey> watcherEvent, CancellationToken ctx)
    {
        TResource? resource = watcherEvent.Resource;
        
        if (resource is null)
        {
            logger.LogWarning(
                "ProcessAddEvent - KubernetesObject is null for {watcherKey} - {kubernetesObjectType}. Ignoring",
                watcherEvent.Key,
                typeof(TResource).Name
            );
            return;
        }

        await resourceStore.Upsert(resource, ctx);    
    }

    private async Task ProcessDeleteEvent(WatcherEvent<TResource, TKey> watcherEvent, CancellationToken ctx)
    {
        TResource? resource = watcherEvent.Resource;
        Uid? resourceId = watcherEvent.ResourceId;
        
        if (resource is null || resourceId is null)
        {
            logger.LogWarning(
                "ProcessDeleteEvent - KubernetesObject is null for {watcherKey} - {kubernetesObjectType}. Ignoring",
                watcherEvent.Key,
                typeof(TResource).Name
            );
            return;
        }
        
        await resourceStore.Delete(resource.Id, resourceId.Value, ctx);
    }
    
    private async Task ProcessErrorEvent(WatcherEvent<TResource, TKey> watcherEvent, CancellationToken ctx)
    {
        await resourceStore.ClearByNamespace(watcherEvent.Key.NamespaceName, ctx);
    }
    
    private void ProcessInitEvent(WatcherEvent<TResource, TKey> watcherEvent)
    {
        logger.LogDebug("ProcessInitEvent - for {watcherKey} - {kubernetesObjectType}. Nothing to do",
            watcherEvent.Key,
            typeof(TResource).Name);
    }
}
