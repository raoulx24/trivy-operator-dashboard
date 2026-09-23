namespace TrivyOperator.Dashboard.Application.Shared.EventDispatchers.Abstractions;

public interface IEventDispatcher<TResource>
{
    void StartEventsProcessing(CancellationToken ctx = default);
}
