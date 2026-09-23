using TrivyOperator.Dashboard.Application.Kubernetes.EventPipeline.EventProcessors.Abstractions;
using TrivyOperator.Dashboard.Application.Kubernetes.Models;
using TrivyOperator.Dashboard.Application.Metrics.Abstractions;
using TrivyOperator.Dashboard.Domain.Trivy.Entities.Abstracts;

namespace TrivyOperator.Dashboard.Application.Kubernetes.EventPipeline.EventProcessors;

public class EventPipelineMetricsProcessor<TResource, TKey> (
    IMetricsClient metricsClient,
    ILogger<EventPipelineMetricsProcessor<TResource, TKey>> logger
) : IKubernetesEventProcessor<TResource, TKey>
    where TResource : class, ITrivyReport<TKey>
{
    public Task ProcessKubernetesEvent(
        WatcherEvent<TResource, TKey> watcherEvent,
        CancellationToken ctx
    )
    {
        switch (watcherEvent.WatcherEventType)
        {
            case WatcherEventType.InitialAdded:
            case WatcherEventType.Added:
            case WatcherEventType.Deleted:
            case WatcherEventType.Modified:
            case WatcherEventType.Bookmark:
            case WatcherEventType.Error:
            case WatcherEventType.Flushed:
            case WatcherEventType.Initialized:
            case WatcherEventType.Unknown:
                ProcessEvent(watcherEvent, ctx);
                break;
            case WatcherEventType.WatcherConnected:
                break;
            default:
                logger.LogWarning(
                    "Unknown event type {eventType} for {kubernetesObjectType}.",
                    watcherEvent.WatcherEventType,
                    typeof(TResource).Name
                );
                break;
        }
        
        return Task.CompletedTask;
    }
    
    private void ProcessEvent(WatcherEvent<TResource, TKey> watcherEvent, CancellationToken ctx)
    {
        ctx.ThrowIfCancellationRequested();
        
        metricsClient.WatcherProcessedMessagesCounter.Add(
            1,
            new KeyValuePair<string, object?>("resource_kind", typeof(TResource).Name),
            new KeyValuePair<string, object?>(
                "resource_level",
                watcherEvent.Key.NamespaceName.IsClusterScoped ? "cluster_scoped" : "namespaced"
            ),
            new KeyValuePair<string, object?>(
                "context_name",
                watcherEvent.Key.ContextName.IsUnset ? null : watcherEvent.Key.ContextName.Value
            ),
            new KeyValuePair<string, object?>(
                "namespace_name",
                watcherEvent.Key.NamespaceName.IsClusterScoped ? null : watcherEvent.Key.NamespaceName.Value
            ),
            new KeyValuePair<string, object?>("watch_event_type", watcherEvent.WatcherEventType.ToString())
        );
    }
}
