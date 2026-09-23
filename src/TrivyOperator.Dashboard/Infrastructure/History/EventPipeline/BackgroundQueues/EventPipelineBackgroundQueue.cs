using Microsoft.Extensions.Options;
using TrivyOperator.Dashboard.Application.History.VulnerabilityReportsHistory.Models;
using TrivyOperator.Dashboard.Domain.History.VulnerabilityReportsHistory.Entities;
using TrivyOperator.Dashboard.Infrastructure.BackgroundQueues;

namespace TrivyOperator.Dashboard.Infrastructure.History.EventPipeline.BackgroundQueues;

public class EventPipelineBackgroundQueue<TResource>(
    IOptions<BackgroundQueueOptions> options,
    ILogger<EventPipelineBackgroundQueue<TResource>> localLogger
) : BackgroundQueue<HistoryEvent<Snapshot>>(options, localLogger);
