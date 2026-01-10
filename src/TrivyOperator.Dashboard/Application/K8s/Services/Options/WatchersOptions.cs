namespace TrivyOperator.Dashboard.Application.K8s.Services.Options;

public class WatchersOptions
{
    public int WatchTimeoutInSeconds { get; init; } = 300;
    public bool FilterWatchersWithNoActivity { get; init; } = true;
}
