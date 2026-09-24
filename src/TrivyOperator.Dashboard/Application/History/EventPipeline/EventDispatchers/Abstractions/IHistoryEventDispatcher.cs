using TrivyOperator.Dashboard.Application.Shared.EventDispatchers.Abstractions;

namespace TrivyOperator.Dashboard.Application.History.EventPipeline.EventDispatchers.Abstractions;

public interface IHistoryEventDispatcher<THistoryResource> : IEventDispatcher<THistoryResource>;
