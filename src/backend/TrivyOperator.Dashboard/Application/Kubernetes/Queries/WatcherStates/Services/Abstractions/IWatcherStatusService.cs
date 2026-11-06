using TrivyOperator.Dashboard.Application.Kubernetes.Queries.WatcherStates.Models;
using TrivyOperator.Dashboard.Application.Shared.Queries.Models;
using TrivyOperator.Dashboard.Application.Shared.Queries;
using TrivyOperator.Dashboard.Application.Shared.Queries.Services.Abstractions;

namespace TrivyOperator.Dashboard.Application.Kubernetes.Queries.WatcherStates.Services.Abstractions;

public interface IWatcherStatusService : IQueryService
{
    Task<IEnumerable<WatcherStatusDto>> GetWatcherStatusDtos();
    Task<OperationResult> RecreateWatcher(string kubernetesObjectType, string? contextName, string namespaceName, CancellationToken ctx = default);
}
