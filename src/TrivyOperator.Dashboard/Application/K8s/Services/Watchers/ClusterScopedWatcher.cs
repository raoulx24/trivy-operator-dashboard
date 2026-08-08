using k8s;
using k8s.Models;
using Microsoft.Extensions.Options;
using TrivyOperator.Dashboard.Application.K8s.Pipeline;
using TrivyOperator.Dashboard.Application.K8s.Services.Options;
using TrivyOperator.Dashboard.Application.K8s.Services.Watchers.Abstractions;
using TrivyOperator.Dashboard.Domain.K8s.ValueObjects;
using TrivyOperator.Dashboard.Infrastructure.Clients.Abstractions;
using TrivyOperator.Dashboard.Infrastructure.K8s.Services.Abstractions;

namespace TrivyOperator.Dashboard.Application.K8s.Services.Watchers;

public class ClusterScopedWatcher<TKubernetesObjectList, TKubernetesObject, TBackgroundQueue>(
    IClusterScopedResourceService<TKubernetesObject, TKubernetesObjectList>
        clusterScopResourceService,
    TBackgroundQueue backgroundQueue,
    IOptions<WatchersOptions> options,
    IMetricsClient metricsClient,
    ILogger<ClusterScopedWatcher<TKubernetesObjectList, TKubernetesObject, TBackgroundQueue>>
        logger
) : KubernetesWatcher<TKubernetesObjectList, TKubernetesObject, TBackgroundQueue>(
    clusterScopResourceService,
    backgroundQueue,
    options,
    metricsClient,
    logger
), IClusterScopedWatcher<TKubernetesObject>
    where TKubernetesObject : class, IKubernetesObject<V1ObjectMeta>, new()
    where TKubernetesObjectList : IKubernetesObject<V1ListMeta>, IItems<TKubernetesObject>
    where TBackgroundQueue : IKubernetesBackgroundQueue<TKubernetesObject>
{
    protected override IAsyncEnumerable<WatchEvent<TKubernetesObject>> GetKubernetesObjectWatchList(
        string watcherKey,
        string? lastResourceVersion,
        Action<Exception>? onError = null,
        CancellationToken cancellationToken = default
    ) => clusterScopResourceService.GetResourceWatchList(
        lastResourceVersion,
        GetWatcherRandomTimeout(),
        onError,
        cancellationToken
    );

    protected override async Task<TKubernetesObjectList> GetInitialResources(
        string watcherKey,
        string? continueToken,
        CancellationToken cancellationToken = default
    ) => await clusterScopResourceService.GetResourceList(
        resourceListPageSize,
        continueToken,
        cancellationToken
    );
}
