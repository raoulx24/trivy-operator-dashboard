using TrivyOperator.Dashboard.Application.Kubernetes.EventPipeline.EventProcessors.Abstractions;
using TrivyOperator.Dashboard.Application.Kubernetes.Models;
using TrivyOperator.Dashboard.Application.Shared.Models;
using TrivyOperator.Dashboard.Domain.Shared.Abstractions;

namespace TrivyOperator.Dashboard.Application.Shared.Details.KubernetesEventProcessors;

// feeds the details store(s) of a family from the reports of the events
// - every detail of an added or modified report is touched, even when unchanged, so the housekeeping can
//   tell the details in use from the orphaned ones (see DetailsOptions)
// - nothing is removed here; the housekeeping hosted service removes the orphaned details
// - register it before the processor that stores the slim report, so the details are there before the report
public abstract class DetailsCollector<TReport, TKey>(
    ILogger<DetailsCollector<TReport, TKey>> logger
) : IKubernetesEventProcessor<TReport, TKey>
    where TReport : class, IEntity<TKey>
{
    public async Task ProcessEvent(KubernetesEvent<TReport, TKey> kubernetesEvent, CancellationToken ctx = default)
    {
        switch (kubernetesEvent.PipelineEventType)
        {
            case PipelineEventType.Added:
            case PipelineEventType.Modified:
            case PipelineEventType.InitialAdded:
                if (kubernetesEvent.Resource is { } report)
                {
                    await Collect(report, ctx);
                }

                break;
            case PipelineEventType.Initialized:
            case PipelineEventType.Deleted:
            case PipelineEventType.Error:
            case PipelineEventType.Flushed:
            case PipelineEventType.Bookmark:
            case PipelineEventType.WatcherConnected:
            case PipelineEventType.Throttled:
                break;
            case PipelineEventType.Unknown:
            default:
                logger.LogWarning(
                    "Unknown event type {eventType} for {kubernetesObjectType}",
                    kubernetesEvent.PipelineEventType,
                    typeof(TReport).Name);
                break;
        }
    }

    // upserts every distinct detail of the report
    protected abstract Task Collect(TReport report, CancellationToken ctx);
}
