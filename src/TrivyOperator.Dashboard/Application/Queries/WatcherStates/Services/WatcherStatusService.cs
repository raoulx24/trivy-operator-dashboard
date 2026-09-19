using k8s.Models;
using Microsoft.Extensions.Options;
using TrivyOperator.Dashboard.Application.KubernetesEventPipeline.Models.WatcherEvents;
using TrivyOperator.Dashboard.Application.KubernetesEventPipeline.Services.Options;
using TrivyOperator.Dashboard.Application.KubernetesEventPipeline.Services.WatcherRegistries.Abstractions;
using TrivyOperator.Dashboard.Application.Queries.Common.Models;
using TrivyOperator.Dashboard.Application.Queries.WatcherStates.Models;
using TrivyOperator.Dashboard.Application.Queries.WatcherStates.Services.Abstractions;
using TrivyOperator.Dashboard.Application.WatcherStates.Models;
using TrivyOperator.Dashboard.Domain.Kubernetes.ValueObjects;
using TrivyOperator.Dashboard.Infrastructure.Caching.ConcurrentCache.Abstractions;
using TrivyOperator.Dashboard.Infrastructure.Trivy.Factories;

namespace TrivyOperator.Dashboard.Application.Queries.WatcherStates.Services;

public class WatcherStatusService(
    IConcurrentCache<WatcherKey, WatcherStateInfo> cache,
    IOptions<WatchersOptions> options,
    IEnumerable<IClusterScopedWatcherRegistry> clusterScopedWatchers,
    IEnumerable<INamespacedWatcherRegistry> namespacedWatchers
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

        return Task.FromResult<IEnumerable<WatcherStatusDto>>(cachedValues);
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

        IKubernetesWatcherRegistry? watcherRegistry =
            clusterScopedWatchers.FirstOrDefault(x => x.WatchedKubernetesObjectType == watchedKubernetesType)
            ?? (IKubernetesWatcherRegistry?)namespacedWatchers.FirstOrDefault(x => x.WatchedKubernetesObjectType == watchedKubernetesType);

        WatcherKey watcherKey = new(new ContextName(contextName), new NamespaceName(namespaceName));
        
        if (watcherRegistry is null)
        {
            return new OperationResult
            {
                Success = false,
                Message = $"No watcher found for {kubernetesObjectType} in {watcherKey}.",
            };
        }
        
        await watcherRegistry.RecreateWatcher(watcherKey, ctx);

        return new OperationResult
        {
            Success = true,
            Message =
                $"Watcher for {kubernetesObjectType} in {watcherKey} has been recreated.",
        };

    }
}
