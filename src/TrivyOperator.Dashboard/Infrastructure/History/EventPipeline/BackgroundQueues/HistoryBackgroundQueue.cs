using Microsoft.Extensions.Options;
using TrivyOperator.Dashboard.Application.History.EventPipeline.BackgroundQueue;
using TrivyOperator.Dashboard.Application.History.VulnerabilityReportsHistory.Models;
using TrivyOperator.Dashboard.Infrastructure.BackgroundQueues;

namespace TrivyOperator.Dashboard.Infrastructure.History.EventPipeline.BackgroundQueues;

public class HistoryBackgroundQueue<THistoryResource>(
    IOptions<BackgroundQueueOptions> options,
    ILogger<HistoryBackgroundQueue<THistoryResource>> localLogger
) : BackgroundQueue<HistoryEvent<THistoryResource>>(options, localLogger), IHistoryBackgroundQueue<THistoryResource>;
