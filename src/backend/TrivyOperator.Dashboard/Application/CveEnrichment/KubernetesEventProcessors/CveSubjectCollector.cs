using TrivyOperator.Dashboard.Application.Kubernetes.EventPipeline.EventProcessors.Abstractions;
using TrivyOperator.Dashboard.Application.Kubernetes.Models;
using TrivyOperator.Dashboard.Application.Shared.Cache.Abstractions;
using TrivyOperator.Dashboard.Application.Shared.Models;
using TrivyOperator.Dashboard.Domain.CveEnrichment.ValueObjects;
using TrivyOperator.Dashboard.Domain.Shared.ValueObjects;
using TrivyOperator.Dashboard.Domain.Trivy.Entities.Abstracts;
using TrivyOperator.Dashboard.Domain.Trivy.ValueObjects.Vulnerabilities;

namespace TrivyOperator.Dashboard.Application.CveEnrichment.KubernetesEventProcessors;

// collects the distinct CVEs seen in vulnerability reports; the value is the newest scan timestamp of a report containing it
// - only moves forward, so an older report (or a late event) does not make a CVE look older than it is
// - nothing is removed here; CVEs not seen for a while are swept by the housekeeping hosted service
public sealed class CveSubjectCollector<TReport, TKey>(
    ICache<CveId, Timestamp> subjects,
    ILogger<CveSubjectCollector<TReport, TKey>> logger
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

        // a report without a scan timestamp would be swept right away
        Timestamp lastSeen = report.LastSeenAt.Value == default ? Timestamp.Now() : report.LastSeenAt;

        HashSet<CveId> ids = [];

        foreach (Vulnerability vulnerability in report.Vulnerabilities)
        {
            CveId id = new(vulnerability.Id.Value);

            if (id.IsValid && ids.Add(id))
            {
                Touch(id, lastSeen);
            }
        }
    }

    private void Touch(CveId id, Timestamp lastSeen)
    {
        while (true)
        {
            if (!subjects.TryGetValue(id, out Timestamp current))
            {
                if (subjects.TryAdd(id, lastSeen))
                {
                    logger.LogDebug("New CVE subject {cveId} from {kubernetesObjectType}", id, typeof(TReport).Name);

                    return;
                }

                // added meanwhile
                continue;
            }

            if (current >= lastSeen || subjects.TryUpdate(id, lastSeen, current))
            {
                return;
            }

            // changed meanwhile
        }
    }
}
