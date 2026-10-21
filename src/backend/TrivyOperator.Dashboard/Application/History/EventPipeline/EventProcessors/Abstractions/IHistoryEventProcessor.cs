using TrivyOperator.Dashboard.Application.History.VulnerabilityReportsHistory.Models;
using TrivyOperator.Dashboard.Application.Shared.EventPipeline.EventProcessors.Abstractions;

namespace TrivyOperator.Dashboard.Application.History.EventPipeline.EventProcessors.Abstractions;

public interface IHistoryEventProcessor<THistoryResource> : IEventProcessor<HistoryEvent<THistoryResource>>;
