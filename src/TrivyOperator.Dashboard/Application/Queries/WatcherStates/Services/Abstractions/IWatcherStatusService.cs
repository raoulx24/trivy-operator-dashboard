using TrivyOperator.Dashboard.Application.Queries.Common.Models;
using TrivyOperator.Dashboard.Application.Queries.WatcherStates.Models;

namespace TrivyOperator.Dashboard.Application.Queries.WatcherStates.Services.Abstractions;

public interface IWatcherStatusService
{
    Task<IEnumerable<WatcherStatusDto>> GetWatcherStatusDtos();
    Task<OperationResult> RecreateWatcher(string kubernetesObjectType, string? contextName, string namespaceName, CancellationToken ctx = default);
}
