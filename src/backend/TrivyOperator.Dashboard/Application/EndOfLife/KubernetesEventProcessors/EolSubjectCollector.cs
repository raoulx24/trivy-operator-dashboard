using TrivyOperator.Dashboard.Application.Kubernetes.EventPipeline.EventProcessors.Abstractions;
using TrivyOperator.Dashboard.Application.Kubernetes.Models;
using TrivyOperator.Dashboard.Application.Shared.Cache.Abstractions;
using TrivyOperator.Dashboard.Application.Shared.Models;
using TrivyOperator.Dashboard.Domain.EndOfLife.ValueObjects;
using TrivyOperator.Dashboard.Domain.Shared.ValueObjects;
using TrivyOperator.Dashboard.Domain.Trivy.Entities.Abstracts;

namespace TrivyOperator.Dashboard.Application.EndOfLife.KubernetesEventProcessors;

// collects the distinct OSes seen in vulnerability reports; the value is the moment the subject was first seen
// subjects are never removed - the number of distinct OSes in a cluster is insignificant
public sealed class EolSubjectCollector<TReport, TKey>(
    ICache<EolSubject, Timestamp> subjects,
    ILogger<EolSubjectCollector<TReport, TKey>> logger
) : IKubernetesEventProcessor<TReport, TKey>
    where TReport : class, IVulnerabilityReport<TReport, TKey>
{
    public Task ProcessEvent(KubernetesEvent<TReport, TKey> kubernetesEvent, CancellationToken ctx = default)
    {
        switch (kubernetesEvent.PipelineEventType)
        {
            case PipelineEventType.Added:
            case PipelineEventType.Modified:
            case PipelineEventType.InitialAdded:
                HandleUpsert(kubernetesEvent);
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

        return Task.CompletedTask;
    }

    private void HandleUpsert(KubernetesEvent<TReport, TKey> kubernetesEvent)
    {
        TReport? report = kubernetesEvent.Resource;

        if (report is null)
        {
            return;
        }

        EolSubject subject = EolSubject.FromOs(report.Os);

        if (!subject.IsValid)
        {
            return;
        }

        if (subjects.TryAdd(subject, Timestamp.Now()))
        {
            logger.LogDebug("New end of life subject {subject} from {kubernetesObjectType}", subject, typeof(TReport).Name);
        }
    }
}
