namespace TrivyOperator.Dashboard.Application.Shared.EventPipeline.EventDispatchers.Abstractions;

public interface IEventDispatcher<TResource>
{
    void StartEventsProcessing(CancellationToken ctx = default);
}
