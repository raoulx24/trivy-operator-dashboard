using TrivyOperator.Dashboard.Application.History.VulnerabilityReportsHistory.Models;
using TrivyOperator.Dashboard.Application.Shared.EventPipeline.EventDispatchers.Abstractions;

namespace TrivyOperator.Dashboard.Application.History.EventPipeline.EventDispatchers.Abstractions;

public interface IHistoryEventDispatcher<THistoryResource> : IEventDispatcher<HistoryEvent<THistoryResource>>;
