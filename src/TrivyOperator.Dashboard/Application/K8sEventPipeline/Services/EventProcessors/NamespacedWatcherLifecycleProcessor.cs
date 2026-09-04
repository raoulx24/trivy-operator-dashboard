using k8s.Models;
using TrivyOperator.Dashboard.Application.K8sEventPipeline.Models.WatcherEvents;
using TrivyOperator.Dashboard.Application.K8sEventPipeline.Services.EventProcessors.Abstractions;
using TrivyOperator.Dashboard.Application.K8sEventPipeline.Services.Watchers.Abstractions;
using TrivyOperator.Dashboard.Domain.K8s.Entities;
using TrivyOperator.Dashboard.Domain.K8s.ValueObjects;
using TrivyOperator.Dashboard.Domain.Shared.Stores.Abstractions;

namespace TrivyOperator.Dashboard.Application.K8sEventPipeline.Services.EventProcessors;

public class NamespacedWatcherLifecycleProcessor(
    IEnumerable<INamespacedWatcher> namespacedWatchers,
    IExpiringResourceProvider<K8sNamespace, Uid> resourceProvider,
    ILogger<NamespacedWatcherLifecycleProcessor> logger
    ) : IKubernetesEventProcessor<V1Namespace>
{
    public async Task ProcessKubernetesEvent(
        WatcherEvent<V1Namespace> watcherEvent,
        CancellationToken ctx
    )
    {
        switch (watcherEvent.WatcherEventType)
        {
            case WatcherEventType.InitialAdded:
            case WatcherEventType.Added:
                ProcessAddEvent(watcherEvent, ctx);
                break;
            case WatcherEventType.Deleted:
                await ProcessDeleteEvent(watcherEvent, ctx);
                break;
            case WatcherEventType.Initialized:
                await ProcessInitEvent(watcherEvent, ctx);
                break;
            case WatcherEventType.Error:
            case WatcherEventType.Flushed:
            case WatcherEventType.Modified:
                break;
            case WatcherEventType.Unknown:
                logger.LogWarning(
                    "Unknown event type {eventType} for {kubernetesObjectType}.",
                    watcherEvent.WatcherEventType,
                    nameof(V1Namespace)
                );
                break;
        }
    }
    
    private void ProcessAddEvent(WatcherEvent<V1Namespace> watcherEvent, CancellationToken ctx)
    {
        if (watcherEvent.KubernetesObject is null)
        {
            logger.LogWarning(
                "ProcessAddEvent - KubernetesObject is null for {watcherKey} - {kubernetesObjectType}. Ignoring",
                watcherEvent.Key,
                nameof(V1Namespace)
            );
            return;
        }

        foreach (INamespacedWatcher namespacedWatcher in namespacedWatchers)
        {
            namespacedWatcher.StartWatcher(watcherEvent.Key, ctx);
        }
    }


    private async Task ProcessDeleteEvent(WatcherEvent<V1Namespace> watcherEvent, CancellationToken ctx)
    {
        if (watcherEvent.KubernetesObject == null)
        {
            logger.LogWarning(
                "ProcessAddEvent - KubernetesObject is null for {watcherKey} - {kubernetesObjectType}. Ignoring",
                watcherEvent.Key,
                nameof(V1Namespace)
            );
            return;
        }

        IEnumerable<Task> tasks = namespacedWatchers.Select(s => s.StopWatcher(watcherEvent.Key, ctx));
        await Task.WhenAll(tasks);
    }

    private async Task ProcessInitEvent(WatcherEvent<V1Namespace> watcherEvent, CancellationToken ctx)
    {
        await resourceProvider.Clear(ctx);
        
        IReadOnlyList<K8sNamespace> k8sNamespaces = await resourceProvider.GetResources(ctx);

        NamespaceName[] namespaceNames = [.. k8sNamespaces.Select(x => x.NamespaceName),];
        
        IEnumerable<Task> tasks = namespacedWatchers.Select(s => s.ReconcileNamespaces(watcherEvent.Key.ContextName, namespaceNames, ctx));
        await Task.WhenAll(tasks);
    }
}
