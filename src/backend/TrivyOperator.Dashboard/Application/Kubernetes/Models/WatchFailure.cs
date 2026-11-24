namespace TrivyOperator.Dashboard.Application.Kubernetes.Models;

// what kind of failure happened; processors use it to pick status, alerts and metric tags
public enum WatchFailureCategory
{
    Gone,
    Throttled,
    ServerTransient,
    Transport,
    Unauthorized,
    Forbidden,
    NotFound,
    Unknown,
}

// which step of the watch session failed
public enum WatchPhase
{
    InitialList,
    WatchOpen,
    WatchStream,
}

// one failure of a watch session, plus how long the session has been failing (the streak)
public sealed record WatchFailure(
    WatchFailureCategory Category,
    WatchPhase Phase,
    int? StatusCode,
    TimeSpan? RetryAfter,
    int ConsecutiveFailures,
    DateTime FailingSince,
    Exception Exception
);
