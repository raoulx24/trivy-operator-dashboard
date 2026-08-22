using k8s.Models;
using Microsoft.Extensions.Options;
using TrivyOperator.Dashboard.Application.Common;
using TrivyOperator.Dashboard.Application.K8s.Services.WatcherStates.Abstractions;
using TrivyOperator.Dashboard.Application.K8s.Services.WatcherStates.Models;
using TrivyOperator.Dashboard.Application.K8sEventPipeline.Models.WatcherEvents;
using TrivyOperator.Dashboard.Application.K8sEventPipeline.Services.Options;
using TrivyOperator.Dashboard.Application.K8sEventPipeline.Services.Watchers.Abstractions;
using TrivyOperator.Dashboard.Application.Queries.WatcherStates.Models;
using TrivyOperator.Dashboard.Domain.K8s.ValueObjects;
using TrivyOperator.Dashboard.Infrastructure.Caching.ConcurrentCache.Abstractions;
using TrivyOperator.Dashboard.Infrastructure.Trivy.Factories;

namespace TrivyOperator.Dashboard.Application.Queries.WatcherStates.Services;

public class WatcherStatusService(
    IConcurrentCache<WatcherKey, WatcherStateInfo> cache,
    IOptions<WatchersOptions> options,
    IEnumerable<IClusterScopedWatcher> clusterScopedWatchers,
    IEnumerable<INamespacedWatcher> namespacedWatchers
) : IWatcherStatusService
{
    public Task<IEnumerable<WatcherStatusDto>> GetWatcherStatusDtos()
    {
        WatcherStatusDto[] cachedValues =
        [
            .. cache.Values.Select(x => x.ToWatcherStatusDto())
                .Where(dto =>
                    !options.Value.FilterWatchersWithNoActivity || dto.EventsGauge >= 0 || dto.Status != "Green"
                ),
        ];

        return Task.FromResult((IEnumerable<WatcherStatusDto>)cachedValues);
    }

    public async Task<OperationResult> RecreateWatcher(string kubernetesObjectType, string? contextName, string namespaceName, CancellationToken ctx = default)
    {
        if (string.IsNullOrWhiteSpace(kubernetesObjectType))
        {
            return new OperationResult
            {
                Success = false,
                Message = "KubernetesObjectType is required.",
            };
        }
        
        if (string.IsNullOrWhiteSpace(namespaceName))
        {
            return new OperationResult
            {
                Success = false,
                Message = "NamespaceName is required.",
            };
        }
        
        Type watchedKubernetesType = kubernetesObjectType == "V1Namespace"
            ? typeof(V1Namespace)
            : TrivyReportCrTypeFactory.Get(kubernetesObjectType);

        IKubernetesWatcher? watcher =
            clusterScopedWatchers.FirstOrDefault(x => x.WatchedKubernetesObjectType == watchedKubernetesType)
            ?? (IKubernetesWatcher?)namespacedWatchers.FirstOrDefault(x => x.WatchedKubernetesObjectType == watchedKubernetesType);

        WatcherKey watcherKey = new(new ContextName(contextName), new NamespaceName(namespaceName));
        
        if (watcher is null)
        {
            return new OperationResult
            {
                Success = false,
                Message = $"No watcher found for {kubernetesObjectType} in {watcherKey}.",
            };
        }
        
        await watcher.RecreateWatcher(watcherKey, ctx);

        return new OperationResult
        {
            Success = true,
            Message =
                $"Watcher for {kubernetesObjectType} in {watcherKey} has been recreated.",
        };

    }
}
