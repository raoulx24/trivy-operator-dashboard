using k8s.Models;
using TrivyOperator.Dashboard.Application.Kubernetes.Models;
using TrivyOperator.Dashboard.Application.Kubernetes.WatcherRegistries.Abstractions;
using TrivyOperator.Dashboard.Domain.Kubernetes.Entities;
using TrivyOperator.Dashboard.Domain.Kubernetes.ValueObjects;
using TrivyOperator.Dashboard.Domain.Shared.Stores.Abstractions;
using TrivyOperator.Dashboard.Infrastructure.Kubernetes.EventPipeline.Services.EventProcessors.Abstractions;

namespace TrivyOperator.Dashboard.Infrastructure.Kubernetes.EventPipeline.Services.EventProcessors;

public class NamespacedWatcherLifecycleProcessor(
    IEnumerable<INamespacedWatcherRegistry> namespacedWatcherRegistries,
    IExpiringResourceProvider<KubernetesNamespace, Uid> resourceProvider,
    ILogger<NamespacedWatcherLifecycleProcessor> logger
    ) : IKubernetesEventProcessor<KubernetesNamespace, Uid>
{
    public async Task ProcessKubernetesEvent(
        WatcherEvent<KubernetesNamespace, Uid> watcherEvent,
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
            default:
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
    
    private void ProcessAddEvent(WatcherEvent<KubernetesNamespace, Uid> watcherEvent, CancellationToken ctx)
    {
        if (watcherEvent.Resource is null)
        {
            logger.LogWarning(
                "ProcessAddEvent - KubernetesObject is null for {watcherKey} - {kubernetesObjectType}. Ignoring",
                watcherEvent.Key,
                nameof(KubernetesNamespace)
            );
            return;
        }

        foreach (INamespacedWatcherRegistry namespacedWatcher in namespacedWatcherRegistries)
        {
            namespacedWatcher.StartWatcher(watcherEvent.Key, ctx);
        }
    }


    private async Task ProcessDeleteEvent(WatcherEvent<KubernetesNamespace, Uid> watcherEvent, CancellationToken ctx)
    {
        if (watcherEvent.Resource == null)
        {
            logger.LogWarning(
                "ProcessAddEvent - KubernetesObject is null for {watcherKey} - {kubernetesObjectType}. Ignoring",
                watcherEvent.Key,
                nameof(KubernetesNamespace)
            );
            return;
        }

        IEnumerable<Task> tasks = namespacedWatcherRegistries.Select(s => s.StopWatcher(watcherEvent.Key, ctx));
        await Task.WhenAll(tasks);
    }

    private async Task ProcessInitEvent(WatcherEvent<KubernetesNamespace, Uid> watcherEvent, CancellationToken ctx)
    {
        await resourceProvider.Clear(ctx);
        
        IReadOnlyList<KubernetesNamespace> kubernetesNamespaces = await resourceProvider.GetResources(ctx);

        IReadOnlyCollection<ResourceLocation> keys = 
            [
                .. kubernetesNamespaces.Select(x 
                => new ResourceLocation(watcherEvent.Key.ContextName, x.NamespaceName)),
            ];
        
        IEnumerable<Task> tasks =
            namespacedWatcherRegistries.Select(s => s.Reconcile(keys, ctx));
        
        await Task.WhenAll(tasks);
    }
}
