using TrivyOperator.Dashboard.Domain.Kubernetes.ValueObjects;

namespace TrivyOperator.Dashboard.Application.Kubernetes.WatcherState.Models;

public record WatcherStateInfo(
    WatcherId Id,
    WatcherStateStatus Status,
    Exception? LastException,
    DateTime LastEventMoment,
    int? EventsGauge);


public enum WatcherStateStatus
{
    Green,
    Red,
}