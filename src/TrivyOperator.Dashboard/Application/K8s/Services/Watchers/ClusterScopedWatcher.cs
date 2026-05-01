using k8s;
using k8s.Models;
using Microsoft.Extensions.Options;
using TrivyOperator.Dashboard.Application.K8s.Services.BackgroundQueues.Abstractions;
using TrivyOperator.Dashboard.Application.K8s.Services.Options;
using TrivyOperator.Dashboard.Application.K8s.Services.WatcherEvents.Abstractions;
using TrivyOperator.Dashboard.Application.K8s.Services.Watchers.Abstractions;
using TrivyOperator.Dashboard.Domain.K8s.Abstractions;
using TrivyOperator.Dashboard.Infrastructure.Clients.Abstractions;

namespace TrivyOperator.Dashboard.Application.K8s.Services.Watchers;

public class ClusterScopedWatcher<TKubernetesObjectList, TKubernetesObject, TBackgroundQueue, TKubernetesWatcherEvent>(
    IClusterScopedResourceWatchDomainService<TKubernetesObject, TKubernetesObjectList>
        clusterScopResourceWatchDomainService,
    TBackgroundQueue backgroundQueue,
    IOptions<WatchersOptions> options,
    IMetricsClient metricsClient,
    ILogger<ClusterScopedWatcher<TKubernetesObjectList, TKubernetesObject, TBackgroundQueue, TKubernetesWatcherEvent>>
        logger
) : KubernetesWatcher<TKubernetesObjectList, TKubernetesObject, TBackgroundQueue, TKubernetesWatcherEvent>(
    backgroundQueue,
    options,
    metricsClient,
    logger
), IClusterScopedWatcher<TKubernetesObject>
    where TKubernetesObject : class, IKubernetesObject<V1ObjectMeta>, new()
    where TKubernetesObjectList : IKubernetesObject<V1ListMeta>, IItems<TKubernetesObject>
    where TKubernetesWatcherEvent : IWatcherEvent<TKubernetesObject>, new()
    where TBackgroundQueue : IKubernetesBackgroundQueue<TKubernetesObject>
{
    protected override IAsyncEnumerable<WatchEvent<TKubernetesObject>> GetKubernetesObjectWatchList(
        string watcherKey,
        string? lastResourceVersion,
        Action<Exception>? onError = null,
        CancellationToken? cancellationToken = null
    ) => clusterScopResourceWatchDomainService.GetResourceWatchList(
        lastResourceVersion,
        GetWatcherRandomTimeout(),
        onError,
        cancellationToken
    );

    protected override async Task<TKubernetesObjectList> GetInitialResources(
        string watcherKey,
        string? continueToken,
        CancellationToken? cancellationToken = null
    ) => await clusterScopResourceWatchDomainService.GetResourceList(
        resourceListPageSize,
        continueToken,
        cancellationToken
    );
}
