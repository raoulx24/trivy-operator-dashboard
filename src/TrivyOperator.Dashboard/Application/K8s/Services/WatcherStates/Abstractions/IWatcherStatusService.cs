using TrivyOperator.Dashboard.Application.Common;
using TrivyOperator.Dashboard.Application.K8s.Models;

namespace TrivyOperator.Dashboard.Application.K8s.Services.WatcherStates.Abstractions;

public interface IWatcherStatusService
{
    Task<IEnumerable<WatcherStatusDto>> GetWatcherStatusDtos();
    Task<OperationResult> RecreateWatcher(string kubernetesObjectType, string? contextName, string namespaceName, CancellationToken ctx = default);
}
