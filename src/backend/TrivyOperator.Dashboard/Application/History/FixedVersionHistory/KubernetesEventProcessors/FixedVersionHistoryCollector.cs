using Microsoft.Extensions.Options;
using TrivyOperator.Dashboard.Application.Shared.Details.KubernetesEventProcessors;
using TrivyOperator.Dashboard.Application.Shared.Details.Options;
using TrivyOperator.Dashboard.Domain.History.FixedVersionHistory.Entities;
using TrivyOperator.Dashboard.Domain.History.FixedVersionHistory.Services;
using TrivyOperator.Dashboard.Domain.History.FixedVersionHistory.Stores.Abstractions;
using TrivyOperator.Dashboard.Domain.Shared.ValueObjects;
using TrivyOperator.Dashboard.Domain.Trivy.Entities.Abstracts;
using TrivyOperator.Dashboard.Domain.Trivy.ValueObjects.Vulnerabilities;

namespace TrivyOperator.Dashboard.Application.History.FixedVersionHistory.KubernetesEventProcessors;

// records the fixed version history per (vulnerability id, package) from the reports of the events
// - the reports come from the events (full, not hydrated): a missing detail cannot look like a removed fix
// - per report, one value per pair: the one of the highest score, as VulnerabilityDetailsCollector does
// - the moment of a value is the report's updateTimestamp (LastSeenAt)
// - best effort: a failure is logged and the event is not retried
public sealed class FixedVersionHistoryCollector<TReport, TKey>(
    IFixedVersionHistoryStore store,
    IOptionsMonitor<DetailsOptions> options,
    ILogger<FixedVersionHistoryCollector<TReport, TKey>> logger
) : DetailsCollector<TReport, TKey>(logger)
    where TReport : class, IVulnerabilityReport<TReport, TKey>
{
    protected override async Task Collect(TReport report, CancellationToken ctx)
    {
        try
        {
            Dictionary<PackageVulnerabilityKey, Vulnerability> reportPairs = [];

            foreach (Vulnerability vulnerability in report.Vulnerabilities)
            {
                PackageVulnerabilityKey key = new(vulnerability.Id, PackageKey.From(vulnerability.ScannedPackage));

                if (!reportPairs.TryGetValue(key, out Vulnerability? current) ||
                    vulnerability.Score > current.Score)
                {
                    reportPairs[key] = vulnerability;
                }
            }

            if (reportPairs.Count == 0)
            {
                return;
            }

            IReadOnlyDictionary<PackageVulnerabilityKey, FixedVersionState> states =
                await store.GetStatesAsync(reportPairs.Keys, ctx);

            Timestamp now = new(DateTime.UtcNow);
            TimeSpan touchRefresh = TimeSpan.FromSeconds(Math.Max(0, options.CurrentValue.TouchRefreshInSeconds));
            List<FixedVersionWrite> writes = [];

            foreach ((PackageVulnerabilityKey key, Vulnerability vulnerability) in reportPairs)
            {
                FixedVersionWrite? write = FixedVersionRecorder.Decide(
                    key,
                    new FixedVersionEntry(vulnerability.FixedVersion, report.LastSeenAt),
                    states.GetValueOrDefault(key),
                    now,
                    touchRefresh);

                if (write is not null)
                {
                    writes.Add(write);
                }
            }

            await store.ApplyAsync(writes, ctx);
        }
        catch (OperationCanceledException) when (ctx.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception ex)
        {
            logger.LogError(
                ex,
                "Failed to record the fixed version history for {kubernetesObjectType} {reportId}",
                typeof(TReport).Name,
                report.Id);
        }
    }
}
