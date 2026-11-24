namespace TrivyOperator.Dashboard.Application.Kubernetes.WatcherState.Options;

public class WatchersOptions
{
    public int WatchTimeoutInSeconds { get; init; } = 300;
    public bool FilterWatchersWithNoActivity { get; init; } = true;
    public int MaxConcurrentInitialLists { get; init; } = 2;
    public int ResourceListPageSize { get; init; } = 100;

    // page size per report type (the EnabledTrivyReports names), for types with large objects (e.g. SbomReport);
    // types not listed use ResourceListPageSize
    public Dictionary<string, int> ResourceListPageSizes { get; init; } = [];

    // backoff for transient failures: exponential from base, capped, with jitter; Retry-After is the minimum
    public int RetryBaseDelaySeconds { get; init; } = 1;
    public int RetryMaxDelaySeconds { get; init; } = 60;

    // fixed retry interval for failures that will not fix themselves soon (403, 404)
    public int PermanentFailureRetrySeconds { get; init; } = 300;

    // a failing watcher turns Red after this many failures in a row, or after failing this long (first reached)
    public int RedAfterConsecutiveFailures { get; init; } = 5;
    public int RedAfterSeconds { get; init; } = 300;
}
