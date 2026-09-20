using System.Diagnostics.Metrics;
using TrivyOperator.Dashboard.Domain.History.VulnerabilityReportsHistory;
using TrivyOperator.Dashboard.Domain.History.VulnerabilityReportsHistory.Entities;
using TrivyOperator.Dashboard.Domain.Kubernetes.ValueObjects;

namespace TrivyOperator.Dashboard.Infrastructure.Clients.Metrics.Abstractions;

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

    // TODO: add context here
    void RecordCveDeltas(NamespaceName sourceNamespaceName, Snapshot snapshot, string resourceKind);
}
