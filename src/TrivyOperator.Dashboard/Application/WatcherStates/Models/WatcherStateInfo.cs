using TrivyOperator.Dashboard.Application.K8s.Models.WatcherEvents;

namespace TrivyOperator.Dashboard.Application.K8s.Services.WatcherStates.Models;

public class WatcherStateInfo
{
    public required Type WatchedKubernetesObjectType { get; init; }
    public WatcherKey Key { get; init; } = new();
    public WatcherStateStatus Status { get; init; }
    public Exception? LastException { get; init; }
    public DateTime LastEventMoment { get; init; } = DateTime.UtcNow;
    public int? EventsGauge { get; init; }
}
