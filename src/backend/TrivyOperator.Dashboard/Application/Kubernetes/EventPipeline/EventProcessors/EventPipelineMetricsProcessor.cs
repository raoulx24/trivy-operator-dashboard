using TrivyOperator.Dashboard.Application.Kubernetes.EventPipeline.EventProcessors.Abstractions;
using TrivyOperator.Dashboard.Application.Kubernetes.Models;
using TrivyOperator.Dashboard.Application.Metrics.Abstractions;
using TrivyOperator.Dashboard.Application.Shared.Models;
using TrivyOperator.Dashboard.Domain.Shared.Abstractions;

namespace TrivyOperator.Dashboard.Application.Kubernetes.EventPipeline.EventProcessors;

public class EventPipelineMetricsProcessor<TResource, TKey> (
    IMetricsClient metricsClient,
    ILogger<EventPipelineMetricsProcessor<TResource, TKey>> logger
) : IKubernetesEventProcessor<TResource, TKey>
    where TResource : class, IEntity<TKey>
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
            case PipelineEventType.Throttled:
            case PipelineEventType.Skipped:
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
            // the watcher: cluster wide or per namespace
            new KeyValuePair<string, object?>(
                "resource_level",
                kubernetesEvent.Scope.IsClusterWide ? "cluster_scoped" : "namespaced"
            ),
            new KeyValuePair<string, object?>(
                "context_name",
                kubernetesEvent.Scope.ContextName.IsUnset ? null : kubernetesEvent.Scope.ContextName.Value
            ),
            // the resource: where it lives (the scope's namespace for events without a resource)
            new KeyValuePair<string, object?>(
                "namespace_name",
                kubernetesEvent.Location.NamespaceName.IsClusterScoped ? null : kubernetesEvent.Location.NamespaceName.Value
            ),
            new KeyValuePair<string, object?>("watch_event_type", kubernetesEvent.PipelineEventType.ToString()),
            // set on Error events only
            new KeyValuePair<string, object?>("failure_category", kubernetesEvent.Failure?.Category.ToString()),
            new KeyValuePair<string, object?>("status_code", kubernetesEvent.Failure?.StatusCode)
        );
    }
}
