namespace TrivyOperator.Dashboard.Application.Shared.EventPipeline.EventPipelineStarters.Abstractions;

// starts watchers that publish into several pipelines; called once all event pipelines are started
public interface IWatchersStarter
{
    Task StartWatchers(CancellationToken ctx = default);
}
