namespace TrivyOperator.Dashboard.Application.Shared.EventPipeline.EventPipelineStarters.Abstractions;

public interface IEventPipelineStarter
{
    void StartPipeline(CancellationToken ctx = default);
}
