using TrivyOperator.Dashboard.Application.History.EventPipeline.BackgroundQueue;
using TrivyOperator.Dashboard.Application.History.EventPipeline.EventDispatchers.Abstractions;
using TrivyOperator.Dashboard.Application.History.EventPipeline.EventProcessors.Abstractions;
using TrivyOperator.Dashboard.Application.History.VulnerabilityReportsHistory.Models;
using TrivyOperator.Dashboard.Application.Shared.EventDispatchers;

namespace TrivyOperator.Dashboard.Application.History.EventPipeline.EventDispatchers;

public class HistoryEventDispatcher<THistoryResource>(
    IEnumerable<IHistoryEventProcessor<THistoryResource>> services,
    IHistoryBackgroundQueue<THistoryResource> backgroundQueue,
    ILogger<HistoryEventDispatcher<THistoryResource>> logger
) : EventDispatcher<THistoryResource, HistoryEvent<THistoryResource>>(services, backgroundQueue, logger),
    IHistoryEventDispatcher<THistoryResource>;
