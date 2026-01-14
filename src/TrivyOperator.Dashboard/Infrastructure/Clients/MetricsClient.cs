using System.Diagnostics.Metrics;
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
    }

    public string AppName { get; }

    public Counter<long> WatcherProcessedMessagesCounter { get; }

    public void CreateObservableGauge(
        string name,
        Func<IEnumerable<Measurement<long>>> observeValues,
        string? unit,
        string? description
    ) => meter.CreateObservableGauge(name, observeValues, unit, description);
}
