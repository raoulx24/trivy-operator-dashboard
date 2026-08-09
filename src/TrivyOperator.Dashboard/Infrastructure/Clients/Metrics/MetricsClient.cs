using System.Diagnostics.Metrics;
using TrivyOperator.Dashboard.Domain.History.VulnerabilityReportsHistory;
using TrivyOperator.Dashboard.Domain.TrivyOld;
using TrivyOperator.Dashboard.Infrastructure.Clients.Metrics.Abstractions;

namespace TrivyOperator.Dashboard.Infrastructure.Clients.Metrics;

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
    private Counter<long> CveChangesCounter { get; }

    public void CreateObservableGauge(
        string name,
        Func<IEnumerable<Measurement<long>>> observeValues,
        string? unit,
        string? description
    ) => meter.CreateObservableGauge(name, observeValues, unit, description);
    
    public void RecordCveDeltas(Snapshot snapshot, string resourceKind)
    {
        string namespaceName = snapshot.Key.NamespaceName.Value;
        int addedLength = snapshot.Metadata.AddedCvesDeltas.Length;
        int droppedLength = snapshot.Metadata.DroppedCvesDeltas.Length;

        foreach (TrivySeverity severity in Enum.GetValues<TrivySeverity>())
        {
            int index = (int)severity;
            int added = addedLength >= index ? snapshot.Metadata.AddedCvesDeltas[index] : 0;
            int dropped = droppedLength >= index ? snapshot.Metadata.DroppedCvesDeltas[index] : 0;

            if (added == 0 && dropped == 0)
                continue;

            string severityLabel = severity.ToString().ToLowerInvariant();

            if (added > 0)
            {
                CveChangesCounter.Add(
                    added,
                    [
                        new KeyValuePair<string, object?>("resource_kind", resourceKind),
                        new KeyValuePair<string, object?>("namespace_name", namespaceName),
                        new KeyValuePair<string, object?>("severity", severityLabel),
                        new KeyValuePair<string, object?>("change_type", "added"),
                    ]
                );
            }

            if (dropped > 0)
            {
                CveChangesCounter.Add(
                    dropped,
                    [
                        new KeyValuePair<string, object?>("resource_kind", resourceKind),
                        new KeyValuePair<string, object?>("namespace_name", namespaceName),
                        new KeyValuePair<string, object?>("severity", severityLabel),
                        new KeyValuePair<string, object?>("change_type", "dropped"),
                    ]
                );
            }
        }
    }
}
