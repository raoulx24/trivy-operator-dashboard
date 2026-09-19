using TrivyOperator.Dashboard.Domain.Kubernetes.ValueObjects;

namespace TrivyOperator.Dashboard.Application.Kubernetes.WatcherState.Models;

public class WatcherStateInfo
{
    public required Type WatchedKubernetesObjectType { get; init; }
    public ResourceLocation Key { get; init; } = new();
    public WatcherStateStatus Status { get; init; }
    public Exception? LastException { get; init; }
    public DateTime LastEventMoment { get; init; } = DateTime.UtcNow;
    public int? EventsGauge { get; init; }
}

public enum WatcherStateStatus
{
    Green,
    Red,
}