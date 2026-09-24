using TrivyOperator.Dashboard.Application.History.VulnerabilityReportsHistory.Models;
using TrivyOperator.Dashboard.Application.Shared.EventProcessors.Abstractions;

namespace TrivyOperator.Dashboard.Application.History.EventProcessors.Abstractions;

public interface IHistoryEventProcessor<THistoryResource> : IEventProcessor<HistoryEvent<THistoryResource>>;
