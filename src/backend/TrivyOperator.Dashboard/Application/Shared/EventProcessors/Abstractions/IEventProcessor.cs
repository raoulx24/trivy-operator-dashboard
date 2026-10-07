namespace TrivyOperator.Dashboard.Application.Shared.EventProcessors.Abstractions;

public interface IEventProcessor<in TEvent>
{
    Task ProcessEvent(TEvent pipeEvent, CancellationToken ctx = default);
}
