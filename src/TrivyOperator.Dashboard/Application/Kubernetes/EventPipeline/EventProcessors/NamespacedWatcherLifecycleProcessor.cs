using k8s.Models;
using TrivyOperator.Dashboard.Application.Kubernetes.EventPipeline.EventProcessors.Abstractions;
using TrivyOperator.Dashboard.Application.Kubernetes.Models;
using TrivyOperator.Dashboard.Application.Kubernetes.WatcherRegistries.Abstractions;
using TrivyOperator.Dashboard.Application.Shared.Models;
using TrivyOperator.Dashboard.Domain.Kubernetes.Entities;
using TrivyOperator.Dashboard.Domain.Kubernetes.ValueObjects;
using TrivyOperator.Dashboard.Domain.Shared.Stores.Abstractions;

namespace TrivyOperator.Dashboard.Application.Kubernetes.EventPipeline.EventProcessors;

public class NamespacedWatcherLifecycleProcessor(
    IEnumerable<INamespacedWatcherRegistry> namespacedWatcherRegistries,
    IExpiringResourceProvider<KubernetesNamespace, Uid> resourceProvider,
    ILogger<NamespacedWatcherLifecycleProcessor> logger
    ) : IKubernetesEventProcessor<KubernetesNamespace, Uid>
{
    public async Task ProcessEvent(
        KubernetesEvent<KubernetesNamespace, Uid> kubernetesEvent,
        CancellationToken ctx
    )
    {
        switch (kubernetesEvent.PipelineEventType)
        {
            case PipelineEventType.InitialAdded:
            case PipelineEventType.Added:
                ProcessAddEvent(kubernetesEvent, ctx);
                break;
            case PipelineEventType.Deleted:
                await ProcessDeleteEvent(kubernetesEvent, ctx);
                break;
            case PipelineEventType.Initialized:
                await ProcessInitEvent(kubernetesEvent, ctx);
                break;
            case PipelineEventType.Error:
            case PipelineEventType.Flushed:
            case PipelineEventType.Modified:
            default:
                break;
            case PipelineEventType.Unknown:
                logger.LogWarning(
                    "Unknown event type {eventType} for {kubernetesObjectType}.",
                    kubernetesEvent.PipelineEventType,
                    nameof(V1Namespace)
                );
                break;
        }
    }
    
    private void ProcessAddEvent(KubernetesEvent<KubernetesNamespace, Uid> kubernetesEvent, CancellationToken ctx)
    {
        if (kubernetesEvent.Resource is null)
        {
            logger.LogWarning(
                "ProcessAddEvent - KubernetesObject is null for {watcherKey} - {kubernetesObjectType}. Ignoring",
                kubernetesEvent.Key,
                nameof(KubernetesNamespace)
            );
            return;
        }

        foreach (INamespacedWatcherRegistry namespacedWatcher in namespacedWatcherRegistries)
        {
            namespacedWatcher.StartWatcher(kubernetesEvent.Key, ctx);
        }
    }


    private async Task ProcessDeleteEvent(KubernetesEvent<KubernetesNamespace, Uid> kubernetesEvent, CancellationToken ctx)
    {
        if (kubernetesEvent.Resource == null)
        {
            logger.LogWarning(
                "ProcessAddEvent - KubernetesObject is null for {watcherKey} - {kubernetesObjectType}. Ignoring",
                kubernetesEvent.Key,
                nameof(KubernetesNamespace)
            );
            return;
        }

        IEnumerable<Task> tasks = namespacedWatcherRegistries.Select(s => s.StopWatcher(kubernetesEvent.Key, ctx));
        await Task.WhenAll(tasks);
    }

    private async Task ProcessInitEvent(KubernetesEvent<KubernetesNamespace, Uid> kubernetesEvent, CancellationToken ctx)
    {
        await resourceProvider.Clear(ctx);
        
        IReadOnlyList<KubernetesNamespace> kubernetesNamespaces = await resourceProvider.GetResources(ctx);

        IReadOnlyCollection<ResourceLocation> keys = 
            [
                .. kubernetesNamespaces.Select(x 
                => new ResourceLocation(kubernetesEvent.Key.ContextName, x.NamespaceName)),
            ];
        
        IEnumerable<Task> tasks =
            namespacedWatcherRegistries.Select(s => s.Reconcile(keys, ctx));
        
        await Task.WhenAll(tasks);
    }
}
