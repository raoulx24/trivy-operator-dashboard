using k8s;
using k8s.Models;
using Microsoft.Extensions.Options;
using TrivyOperator.Dashboard.Application.K8s.Models.WatcherEvents;
using TrivyOperator.Dashboard.Application.K8s.Pipeline;
using TrivyOperator.Dashboard.Application.K8s.Services.Options;
using TrivyOperator.Dashboard.Application.K8s.Services.Watchers.Abstractions;
using TrivyOperator.Dashboard.Domain.K8s.ValueObjects;
using TrivyOperator.Dashboard.Infrastructure.Clients.Metrics.Abstractions;
using TrivyOperator.Dashboard.Infrastructure.K8s.Services.Abstractions;

namespace TrivyOperator.Dashboard.Application.K8s.Services.Watchers;

public class ClusterScopedWatcher<TKubernetesObjectList, TKubernetesObject>(
    IClusterScopedResourceService<TKubernetesObject, TKubernetesObjectList> clusterScopResourceService,
    IKubernetesBackgroundQueue<TKubernetesObject> backgroundQueue,
    IOptions<WatchersOptions> options,
    IMetricsClient metricsClient,
    ILogger<ClusterScopedWatcher<TKubernetesObjectList, TKubernetesObject>>
        logger
) : KubernetesWatcher<TKubernetesObjectList, TKubernetesObject>(
    backgroundQueue,
    options,
    metricsClient,
    logger
), IClusterScopedWatcher
    where TKubernetesObject : class, IKubernetesObject<V1ObjectMeta>, new()
    where TKubernetesObjectList : IKubernetesObject<V1ListMeta>, IItems<TKubernetesObject>
{
    protected override IAsyncEnumerable<WatchEvent<TKubernetesObject>> GetKubernetesObjectWatchList(
        WatcherKey key,
        string? lastResourceVersion,
        CancellationToken cancellationToken = default
    ) => clusterScopResourceService.GetResourceWatchList(
        lastResourceVersion,
        GetWatcherRandomTimeout(),
        cancellationToken
    );

    protected override async Task<TKubernetesObjectList> GetInitialResources(
        WatcherKey key,
        string? continueToken,
        CancellationToken cancellationToken = default
    ) => await clusterScopResourceService.GetResourceList(
        ResourceListPageSize,
        continueToken,
        cancellationToken
    );
}
