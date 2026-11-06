using TrivyOperator.Dashboard.Application.Kubernetes.Queries.WatcherStates.Models;
using TrivyOperator.Dashboard.Application.Kubernetes.Queries.WatcherStates.Services.Abstractions;
using TrivyOperator.Dashboard.Application.Shared.Queries;
using TrivyOperator.Dashboard.Application.Shared.Queries.Models;
using TrivyOperator.Dashboard.Application.Shared.Queries.Services;

namespace TrivyOperator.Dashboard.Application.Kubernetes.Queries.WatcherStates.Services;

// watcher states are disabled - no watchers to report on
public class WatcherStatusNullService : QueryNullService, IWatcherStatusService
{
    public Task<IEnumerable<WatcherStatusDto>> GetWatcherStatusDtos() =>
        Task.FromResult<IEnumerable<WatcherStatusDto>>([]);

    public Task<OperationResult> RecreateWatcher(
        string kubernetesObjectType,
        string? contextName,
        string namespaceName,
        CancellationToken ctx = default) =>
        Task.FromResult(new OperationResult { Success = false, Message = "Watcher states are disabled." });
}
