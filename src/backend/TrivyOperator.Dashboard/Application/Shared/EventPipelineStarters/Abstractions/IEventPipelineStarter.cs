namespace TrivyOperator.Dashboard.Application.Shared.EventPipelineStarters.Abstractions;

public interface IEventPipelineStarter
{
    void StartPipeline(CancellationToken ctx = default);
}
