using TrivyOperator.Dashboard.Application.Kubernetes.EventPipeline.EventProcessors.Abstractions;
using TrivyOperator.Dashboard.Application.Kubernetes.Models;
using TrivyOperator.Dashboard.Application.Shared.Models;
using TrivyOperator.Dashboard.Domain.Shared.Stores.Abstractions;
using TrivyOperator.Dashboard.Domain.Trivy.Entities.Abstracts;
using TrivyOperator.Dashboard.Domain.Trivy.ValueObjects.SecurityAssessments;

namespace TrivyOperator.Dashboard.Application.Trivy.SecurityAssessmentDetails.KubernetesEventProcessors;

// feeds the checks store from the reports of the events
// - every check of an added or modified report is touched, even when unchanged, so the housekeeping can
//   tell the checks in use from the orphaned ones (see SecurityAssessmentDetailsOptions)
// - nothing is removed here; the housekeeping hosted service removes the orphaned checks
// - register it before the processor that stores the slim report, so the checks are there before the report
public sealed class SecurityAssessmentDetailsCollector<TReport, TKey>(
    IDetailStore<Check, CheckId> details,
    ILogger<SecurityAssessmentDetailsCollector<TReport, TKey>> logger
) : IKubernetesEventProcessor<TReport, TKey>
    where TReport : class, ISecurityAssessmentReport<TReport, TKey>
{
    public async Task ProcessEvent(KubernetesEvent<TReport, TKey> kubernetesEvent, CancellationToken ctx = default)
    {
        switch (kubernetesEvent.PipelineEventType)
        {
            case PipelineEventType.Added:
            case PipelineEventType.Modified:
            case PipelineEventType.InitialAdded:
                await HandleUpsert(kubernetesEvent, ctx);
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

    private async Task HandleUpsert(KubernetesEvent<TReport, TKey> kubernetesEvent, CancellationToken ctx)
    {
        TReport? report = kubernetesEvent.Resource;

        if (report is null)
        {
            return;
        }

        // a check failing for several containers is in the report several times
        HashSet<CheckId> ids = [];

        foreach (Check check in report.Checks)
        {
            if (ids.Add(check.CheckId))
            {
                await details.Upsert(check, report.LastSeenAt, ctx);
            }
        }
    }
}
