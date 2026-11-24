using TrivyOperator.Dashboard.Domain.Kubernetes.ValueObjects;

namespace TrivyOperator.Dashboard.Application.Kubernetes.WatcherState.Models;

public record WatcherStateInfo(
    WatcherId Id,
    WatcherStateStatus Status,
    Exception? LastException,
    DateTime LastEventMoment,
    int? EventsGauge,
    WatcherStateReason Reason = WatcherStateReason.None);


public enum WatcherStateStatus
{
    Green,
    Yellow,
    Red,
}

// why a watcher is Yellow (or Red); None for Green
public enum WatcherStateReason
{
    None,
    // waiting for a free slot to start the initial list
    Throttled,
    // the initial list is in progress
    Listing,
    // a failure, retried; not (yet) Red
    Recovering,
    // a failure that turned the watcher Red
    Failed,
}
