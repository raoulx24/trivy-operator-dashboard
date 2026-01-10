namespace TrivyOperator.Dashboard.Application.K8s.Services.WatcherStates;

public class WatcherStateInfo
{
    public required Type WatchedKubernetesObjectType { get; init; }
    public string WatcherKey { get; init; } = string.Empty;
    public WatcherStateStatus Status { get; init; }
    public Exception? LastException { get; init; }
    public DateTime LastEventMoment { get; init; } = DateTime.UtcNow;
    public int? EventsGauge { get; init; }
}
