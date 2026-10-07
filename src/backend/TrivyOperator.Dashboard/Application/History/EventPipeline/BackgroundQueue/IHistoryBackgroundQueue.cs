using TrivyOperator.Dashboard.Application.History.VulnerabilityReportsHistory.Models;
using TrivyOperator.Dashboard.Application.Shared.BackgroundQueues.Abstractions;

namespace TrivyOperator.Dashboard.Application.History.EventPipeline.BackgroundQueue;

public interface IHistoryBackgroundQueue<THistoryResource> : IBackgroundQueue<HistoryEvent<THistoryResource>>;
