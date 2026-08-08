using Microsoft.Extensions.Options;
using TrivyOperator.Dashboard.Application.Common;
using TrivyOperator.Dashboard.Application.K8s.Models;
using TrivyOperator.Dashboard.Application.K8s.Models.WatcherEvents;
using TrivyOperator.Dashboard.Application.K8s.Pipeline;
using TrivyOperator.Dashboard.Application.K8s.Services.Options;
using TrivyOperator.Dashboard.Application.K8s.Services.Watchers.Abstractions;
using TrivyOperator.Dashboard.Application.K8s.Services.WatcherStates.Abstractions;
using TrivyOperator.Dashboard.Domain.K8s.ValueObjects;
using TrivyOperator.Dashboard.Domain.TrivyOld;
using TrivyOperator.Dashboard.Infrastructure.Caching.InMemoryOld.Abstractions;

namespace TrivyOperator.Dashboard.Application.K8s.Services.WatcherStates;

public class WatcherStatusService(
    IConcurrentCache<WatcherKey, WatcherStateInfo> cache,
    IOptions<WatchersOptions> options,
    IServiceProvider serviceProvider
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
        
        string fullTypeName = kubernetesObjectType == "V1Namespace" 
            ? $"k8s.Models.{kubernetesObjectType}" // the only known type that is not a Trivy Report
            : $"{TrivyDomainUtils.TrivyDomainNamespace}.{kubernetesObjectType.TrimEnd('C', 'r')}.{kubernetesObjectType}";

        Type? watchedKubernetesType = Type.GetType(fullTypeName);

        if (watchedKubernetesType == null)
        {
            return new OperationResult
            {
                Success = false,
                Message = $"KubernetesObjectType '{kubernetesObjectType}' is not recognized.",
            };
        }

        Type clusteredScopedWatcherType = typeof(IClusterScopedWatcher<>).MakeGenericType(watchedKubernetesType);
        Type namespacedWatcherType = typeof(INamespacedWatcher<>).MakeGenericType(watchedKubernetesType);

        object? watcherService = string.IsNullOrWhiteSpace(namespaceName) 
            ? serviceProvider.GetServices(clusteredScopedWatcherType).FirstOrDefault() 
            : serviceProvider.GetServices(namespacedWatcherType).FirstOrDefault();

        WatcherKey watcherKey = new WatcherKey(new ContextName(contextName), new NamespaceName(namespaceName));
        
        if (watcherService is IKubernetesWatcher watcher)
        {
            await watcher.Recreate(watcherKey, ctx);

            return new OperationResult
            {
                Success = true,
                Message =
                    $"Watcher for {kubernetesObjectType} in {watcherKey} has been recreated.",
            };
        }

        return new OperationResult
        {
            Success = false,
            Message = $"No watcher found for {kubernetesObjectType} in {watcherKey}.",
        };
    }
}
