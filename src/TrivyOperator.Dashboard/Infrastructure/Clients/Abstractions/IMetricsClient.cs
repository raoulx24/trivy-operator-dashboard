using System.Diagnostics.Metrics;
using TrivyOperator.Dashboard.Domain.History.VulnerabilityReportsHistory;

namespace TrivyOperator.Dashboard.Infrastructure.Clients.Abstractions;

public interface IMetricsClient
{
    string AppName { get; }

    Counter<long> WatcherProcessedMessagesCounter { get; }

    void CreateObservableGauge(
        string name,
        Func<IEnumerable<Measurement<long>>> observeValues,
        string? unit,
        string? description
    );

    void RecordCveDeltas(Snapshot snapshot, string resourceKind);
}
