namespace TrivyOperator.Dashboard.Application.KubernetesEventPipeline.Services.Options;

public class WatchersOptions
{
    public int WatchTimeoutInSeconds { get; init; } = 300;
    public bool FilterWatchersWithNoActivity { get; init; } = true;
}
