using TrivyOperator.Dashboard.Application.History.EventPipeline.BackgroundQueue;
using TrivyOperator.Dashboard.Application.History.EventPipeline.EventDispatchers.Abstractions;
using TrivyOperator.Dashboard.Application.History.VulnerabilityReportsHistory.Models;
using TrivyOperator.Dashboard.Application.Shared.EventPipeline.EventPipelineStarters;

namespace TrivyOperator.Dashboard.Application.History.EventPipeline.EventPipelineStarters;

public class HistoryEventPipelineStarter<THistoryResource>(
    IHistoryEventDispatcher<THistoryResource> eventDispatcher,
    IHistoryBackgroundQueue<THistoryResource> queue,
    ILogger<HistoryEventPipelineStarter<THistoryResource>> logger
) : EventPipelineStarter<HistoryEvent<THistoryResource>>(eventDispatcher, queue, logger);
