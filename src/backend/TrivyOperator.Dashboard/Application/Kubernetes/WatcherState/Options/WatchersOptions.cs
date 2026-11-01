namespace TrivyOperator.Dashboard.Application.Kubernetes.WatcherState.Options;

public class WatchersOptions
{
    public int WatchTimeoutInSeconds { get; init; } = 300;
    public bool FilterWatchersWithNoActivity { get; init; } = true;
    public int MaxConcurrentInitialLists { get; init; } = 2;
    public int ResourceListPageSize { get; init; } = 100;
}
