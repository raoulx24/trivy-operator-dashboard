using TrivyOperator.Dashboard.Application.K8s.Models.WatcherEvents;
using TrivyOperator.Dashboard.Application.K8s.Pipeline;
using TrivyOperator.Dashboard.Domain.K8s.Abstractions;
using TrivyOperator.Dashboard.Domain.K8s.ValueObjects;
using TrivyOperator.Dashboard.Domain.Trivy.Entities.Abstracts;
using TrivyOperator.Dashboard.Infrastructure.K8s.CustomResources;
using TrivyOperator.Dashboard.Infrastructure.Trivy.Mappers.Abstract;
using TrivyOperator.Dashboard.Infrastructure.Trivy.Mappers.Extensions;

namespace TrivyOperator.Dashboard.Application.K8s.Services.EventProcessors;

public class ResourceStoreUpdater<TKubernetesObject, TResource, TKey> (
    IResourceStore<TResource, TKey> resourceStore,
    ITrivyReportMapper<TKubernetesObject, TResource> mapper,
    ITrivyReportKeyProvider<TKubernetesObject, TKey> keyProvider,
    ILogger<ResourceStoreUpdater<TKubernetesObject, TResource, TKey>> logger
) : IKubernetesEventProcessor<TKubernetesObject>
    where TKubernetesObject : CustomResource, new()
    where TResource : ITrivyReport<TKey>, ITrivyReport
{
    public async Task ProcessKubernetesEvent(
        WatcherEvent<TKubernetesObject> watcherEvent,
        CancellationToken ctx
    )
    {
        switch (watcherEvent.WatcherEventType)
        {
            case WatcherEventType.InitialAdded:
            case WatcherEventType.Added:
                await ProcessAddEvent(watcherEvent, ctx);
                break;
            case WatcherEventType.Deleted:
                await ProcessDeleteEvent(watcherEvent, ctx);
                break;
            case WatcherEventType.Error:
            case WatcherEventType.Flushed:
                await ProcessErrorEvent(watcherEvent, ctx);
                break;
            case WatcherEventType.Modified:
                await ProcessModifiedEvent(watcherEvent, ctx);
                break;
            case WatcherEventType.Initialized:
                ProcessInitEvent(watcherEvent);
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
    
    private async Task ProcessAddEvent(WatcherEvent<TKubernetesObject> watcherEvent, CancellationToken ctx)
    {
        TKubernetesObject? k8sObject = watcherEvent.KubernetesObject;
        
        if (k8sObject is null)
        {
            logger.LogWarning(
                "ProcessAddEvent - KubernetesObject is null for {watcherKey} - {kubernetesObjectType}. Ignoring",
                watcherEvent.Key,
                typeof(TKubernetesObject).Name
            );
            return;
        }

        TKey domainKey = keyProvider.GetKey(k8sObject);

        TResource? existing = await resourceStore.Get(domainKey, ctx);
        TResource resource = mapper.MapToDomain(k8sObject, existing);

        await resourceStore.Upsert(resource, ctx);    
    }

    private async Task ProcessDeleteEvent(WatcherEvent<TKubernetesObject> watcherEvent, CancellationToken ctx)
    {
        TKubernetesObject? k8sObject = watcherEvent.KubernetesObject;
        
        if (k8sObject is null)
        {
            logger.LogWarning(
                "ProcessDeleteEvent - KubernetesObject is null for {watcherKey} - {kubernetesObjectType}. Ignoring",
                watcherEvent.Key,
                typeof(TKubernetesObject).Name
            );
            return;
        }
        
        TKey domainKey = keyProvider.GetKey(k8sObject);
        Uid uid = k8sObject.ToUidKey();

        await resourceStore.Delete(domainKey, uid, ctx);
    }
    
    private async Task ProcessErrorEvent(WatcherEvent<TKubernetesObject> watcherEvent, CancellationToken ctx)
    {
        TKubernetesObject? k8sObject = watcherEvent.KubernetesObject;
        
        if (k8sObject is null)
        {
            logger.LogWarning(
                "ProcessErrorEvent - KubernetesObject is null for {watcherKey} - {kubernetesObjectType}. Ignoring",
                watcherEvent.Key,
                typeof(TKubernetesObject).Name
            );
            return;
        }
        
        NamespaceName ns = new(k8sObject.Metadata.NamespaceProperty);

        await resourceStore.ClearByNamespace(ns, ctx);
    }
    
    private async Task ProcessModifiedEvent(
        WatcherEvent<TKubernetesObject> watcherEvent,
        CancellationToken cancellationToken
    )
    {
        logger.LogDebug("ProcessModifiedEvent - redirecting to ProcessAddEvent.");
        await ProcessAddEvent(watcherEvent, cancellationToken);
    }

    private void ProcessInitEvent(WatcherEvent<TKubernetesObject> watcherEvent)
    {
        logger.LogDebug("ProcessInitEvent - for {watcherKey} - {kubernetesObjectType}. Nothing to do",
            watcherEvent.Key,
            typeof(TKubernetesObject).Name);
    }
}
