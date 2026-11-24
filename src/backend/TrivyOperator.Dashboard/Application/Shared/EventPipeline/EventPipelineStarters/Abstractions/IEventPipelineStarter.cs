namespace TrivyOperator.Dashboard.Application.Shared.EventPipeline.EventPipelineStarters.Abstractions;

public interface IEventPipelineStarter
{
    Task StartPipeline(CancellationToken ctx = default);
}
