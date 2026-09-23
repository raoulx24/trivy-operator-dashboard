using TrivyOperator.Dashboard.Application.Kubernetes.EventPipeline.EventProcessors.Abstractions;
using TrivyOperator.Dashboard.Application.Kubernetes.Models;
using TrivyOperator.Dashboard.Application.Shared.Models;
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
    public async Task ProcessEvent(
        KubernetesEvent<TResource, TKey> kubernetesEvent,
        CancellationToken ctx
    )
    {
        switch (kubernetesEvent.PipelineEventType)
        {
            case PipelineEventType.InitialAdded:
            case PipelineEventType.Added:
            case PipelineEventType.Modified:
                await ProcessAddEvent(kubernetesEvent, ctx);
                break;
            case PipelineEventType.Deleted:
                await ProcessDeleteEvent(kubernetesEvent, ctx);
                break;
            case PipelineEventType.Error:
            case PipelineEventType.Flushed:
                await ProcessErrorEvent(kubernetesEvent, ctx);
                break;
            case PipelineEventType.Initialized:
                ProcessInitEvent(kubernetesEvent);
                break;
            case PipelineEventType.Bookmark:
            case PipelineEventType.WatcherConnected:
                break;
            case PipelineEventType.Unknown:
            default:
                logger.LogWarning(
                    "Unknown event type {eventType} for {kubernetesObjectType}.",
                    kubernetesEvent.PipelineEventType,
                    typeof(TResource).Name
                );
                break;
        }
    }
    
    private async Task ProcessAddEvent(KubernetesEvent<TResource, TKey> kubernetesEvent, CancellationToken ctx)
    {
        TResource? resource = kubernetesEvent.Resource;
        
        if (resource is null)
        {
            logger.LogWarning(
                "ProcessAddEvent - KubernetesObject is null for {watcherKey} - {kubernetesObjectType}. Ignoring",
                kubernetesEvent.Key,
                typeof(TResource).Name
            );
            return;
        }

        await resourceStore.Upsert(resource, ctx);    
    }

    private async Task ProcessDeleteEvent(KubernetesEvent<TResource, TKey> kubernetesEvent, CancellationToken ctx)
    {
        TResource? resource = kubernetesEvent.Resource;
        Uid? resourceId = kubernetesEvent.ResourceId;
        
        if (resource is null || resourceId is null)
        {
            logger.LogWarning(
                "ProcessDeleteEvent - KubernetesObject is null for {watcherKey} - {kubernetesObjectType}. Ignoring",
                kubernetesEvent.Key,
                typeof(TResource).Name
            );
            return;
        }
        
        await resourceStore.Delete(resource.Id, resourceId.Value, ctx);
    }
    
    private async Task ProcessErrorEvent(KubernetesEvent<TResource, TKey> kubernetesEvent, CancellationToken ctx)
    {
        await resourceStore.ClearByNamespace(kubernetesEvent.Key.NamespaceName, ctx);
    }
    
    private void ProcessInitEvent(KubernetesEvent<TResource, TKey> kubernetesEvent)
    {
        logger.LogDebug("ProcessInitEvent - for {watcherKey} - {kubernetesObjectType}. Nothing to do",
            kubernetesEvent.Key,
            typeof(TResource).Name);
    }
}
