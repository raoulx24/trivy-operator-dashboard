using Microsoft.Extensions.Options;
using TrivyOperator.Dashboard.Application.Models;
using TrivyOperator.Dashboard.Application.Services.Common;
using TrivyOperator.Dashboard.Application.Services.Options;
using TrivyOperator.Dashboard.Application.Services.Watchers.Abstractions;
using TrivyOperator.Dashboard.Application.Services.WatcherStates.Abstractions;
using TrivyOperator.Dashboard.Domain.Trivy;
using TrivyOperator.Dashboard.Infrastructure.Abstractions;
using TrivyOperator.Dashboard.Utils;

namespace TrivyOperator.Dashboard.Application.Services.WatcherStates;

public class WatcherStatusService(
    IConcurrentCache<string, WatcherStateInfo> cache,
    IOptions<WatchersOptions> options,
    IServiceProvider serviceProvider) : IWatcherStatusService
{
    public Task<IEnumerable<WatcherStatusDto>> GetWatcherStatusDtos()
    {
        WatcherStatusDto[] cachedValues = [.. cache.Values
            .Select(x => x.ToWatcherStatusDto())
            .Where(dto => !options.Value.FilterWatchersWithNoActivity || dto.EventsGauge >= 0 || dto.Status != "Green")];

        return Task.FromResult((IEnumerable<WatcherStatusDto>)cachedValues);
    }

    public async Task<OperationResult> RecreateWatcher(string kubernetesObjectType, string? namespaceName)
    {
        if (string.IsNullOrWhiteSpace(kubernetesObjectType))
        {
            return new OperationResult
            {
                Success = false,
                Message = "KubernetesObjectType is required."
            };
        }

        string fullTypeName;
        if (kubernetesObjectType == "V1Namespace") // the only known type that is not a Trivy Report
        {
            fullTypeName = $"k8s.Models.{kubernetesObjectType}";
        }
        else
        {
            fullTypeName = $"{TrivyDomainUtils.TrivyDomainNamespace}.{kubernetesObjectType.TrimEnd('C', 'r')}.{kubernetesObjectType}";
        }

        Type? watchedKubernetesType = Type.GetType(fullTypeName);

        if (watchedKubernetesType == null)
        {
            return new OperationResult
            {
                Success = false,
                Message = $"KubernetesObjectType '{kubernetesObjectType}' is not recognized."
            };
        }

        Type clusteredScopedWatcherType =
                    typeof(IClusterScopedWatcher<>).MakeGenericType(watchedKubernetesType);
        Type namespacedWatcherType =
            typeof(INamespacedWatcher<>).MakeGenericType(watchedKubernetesType);

        object? watcherService = null;
        if (string.IsNullOrWhiteSpace(namespaceName))
        {
            watcherService = serviceProvider.GetServices(clusteredScopedWatcherType).FirstOrDefault();
        }
        else
        {
            watcherService = serviceProvider.GetServices(namespacedWatcherType).FirstOrDefault();
        }

        if (watcherService is IKubernetesWatcher watcher)
        {
            await watcher.Recreate(new CancellationToken(), namespaceName ?? CacheUtils.DefaultCacheRefreshKey);
            
            return new OperationResult
            {
                Success = true,
                Message = $"Watcher for {kubernetesObjectType} in namespace '{namespaceName ?? "all"}' has been recreated."
            };
        }

        return new OperationResult
        {
            Success = false,
            Message = $"No watcher found for {kubernetesObjectType} in namespace '{namespaceName ?? "all"}'."
        };
    }
}
