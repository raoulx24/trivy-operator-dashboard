using System.Diagnostics;
using System.Diagnostics.Metrics;
using TrivyOperator.Dashboard.Domain.Trivy;
using TrivyOperator.Dashboard.Domain.VulnerabilityReportsHistory;
using TrivyOperator.Dashboard.Infrastructure.Clients.Abstractions;

namespace TrivyOperator.Dashboard.Infrastructure.Clients;

public class MetricsClient : IMetricsClient
{
    private readonly Meter meter;
    
    public MetricsClient(string appName)
    {
        meter = new Meter($"{appName}.metrics");
        AppName = appName;

        WatcherProcessedMessagesCounter = meter.CreateCounter<long>(
            $"{appName}.watcher.processed_messages.count",
            "events",
            "Counts the total number of processed messages in watcher."
        );
        
        CveChangesCounter = meter.CreateCounter<long>(
            $"{appName}.history.cve_changes.count",
            "cves",
            "Counts added and dropped CVEs by severity and namespace."
        );
    }

    public string AppName { get; }

    public Counter<long> WatcherProcessedMessagesCounter { get; }
    public Counter<long> CveChangesCounter { get; }

    public void CreateObservableGauge(
        string name,
        Func<IEnumerable<Measurement<long>>> observeValues,
        string? unit,
        string? description
    ) => meter.CreateObservableGauge(name, observeValues, unit, description);
    
    public void RecordCveDeltas(
        Snapshot snapshot,
        string resourceKind)
    {
        string namespaceName = snapshot.Metadata.NamespaceName.Value;

        foreach (TrivySeverity severity in Enum.GetValues<TrivySeverity>())
        {
            int index = (int)severity;
            string severityLabel = severity.ToString().ToLowerInvariant();

            int added = snapshot.Metadata.AddedCvesDeltas[index];
            int dropped = snapshot.Metadata.DroppedCvesDeltas[index];

            if (added > 0)
            {
                var tags = new TagList
                {
                    { "resource_kind", resourceKind },
                    { "namespace_name", namespaceName },
                    { "severity", severityLabel },
                    { "change_type", "added" }
                };

                CveChangesCounter.Add(added, tags);
            }

            if (dropped > 0)
            {
                var tags = new TagList
                {
                    { "resource_kind", resourceKind },
                    { "namespace_name", namespaceName },
                    { "severity", severityLabel },
                    { "change_type", "dropped" }
                };

                CveChangesCounter.Add(dropped, tags);
            }
        }
    }

}
