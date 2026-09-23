using TrivyOperator.Dashboard.Application.Kubernetes.EventPipeline.EventProcessors.Abstractions;
using TrivyOperator.Dashboard.Application.Kubernetes.Models;
using TrivyOperator.Dashboard.Application.Metrics.Abstractions;
using TrivyOperator.Dashboard.Application.Shared.Models;
using TrivyOperator.Dashboard.Domain.Trivy.Entities.Abstracts;

namespace TrivyOperator.Dashboard.Application.Kubernetes.EventPipeline.EventProcessors;

public class EventPipelineMetricsProcessor<TResource, TKey> (
    IMetricsClient metricsClient,
    ILogger<EventPipelineMetricsProcessor<TResource, TKey>> logger
) : IKubernetesEventProcessor<TResource, TKey>
    where TResource : class, ITrivyReport<TKey>
{
    public Task ProcessEvent(
        KubernetesEvent<TResource, TKey> kubernetesEvent,
        CancellationToken ctx
    )
    {
        switch (kubernetesEvent.PipelineEventType)
        {
            case PipelineEventType.InitialAdded:
            case PipelineEventType.Added:
            case PipelineEventType.Deleted:
            case PipelineEventType.Modified:
            case PipelineEventType.Bookmark:
            case PipelineEventType.Error:
            case PipelineEventType.Flushed:
            case PipelineEventType.Initialized:
            case PipelineEventType.Unknown:
                UpdateMetrics(kubernetesEvent, ctx);
                break;
            case PipelineEventType.WatcherConnected:
                break;
            default:
                logger.LogWarning(
                    "Unknown event type {eventType} for {kubernetesObjectType}.",
                    kubernetesEvent.PipelineEventType,
                    typeof(TResource).Name
                );
                break;
        }
        
        return Task.CompletedTask;
    }
    
    private void UpdateMetrics(KubernetesEvent<TResource, TKey> kubernetesEvent, CancellationToken ctx)
    {
        ctx.ThrowIfCancellationRequested();
        
        metricsClient.WatcherProcessedMessagesCounter.Add(
            1,
            new KeyValuePair<string, object?>("resource_kind", typeof(TResource).Name),
            new KeyValuePair<string, object?>(
                "resource_level",
                kubernetesEvent.Key.NamespaceName.IsClusterScoped ? "cluster_scoped" : "namespaced"
            ),
            new KeyValuePair<string, object?>(
                "context_name",
                kubernetesEvent.Key.ContextName.IsUnset ? null : kubernetesEvent.Key.ContextName.Value
            ),
            new KeyValuePair<string, object?>(
                "namespace_name",
                kubernetesEvent.Key.NamespaceName.IsClusterScoped ? null : kubernetesEvent.Key.NamespaceName.Value
            ),
            new KeyValuePair<string, object?>("watch_event_type", kubernetesEvent.PipelineEventType.ToString())
        );
    }
}
