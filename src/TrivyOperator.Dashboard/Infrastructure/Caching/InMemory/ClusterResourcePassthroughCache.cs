using k8s;
using k8s.Models;
using TrivyOperator.Dashboard.Domain.K8s.Abstractions;
using TrivyOperator.Dashboard.Infrastructure.Caching.InMemory.Abstractions;
using TrivyOperator.Dashboard.Infrastructure.K8s.Services.Abstractions;
using TrivyOperator.Dashboard.Infrastructure.Utils;

namespace TrivyOperator.Dashboard.Infrastructure.Caching.InMemory;

public class ClusterResourcePassthroughCache<TValue, TList>(
    IClusterScopedResourceWatchService<TValue, TList> domain
) : ResourcePassthroughCache<TValue>
    where TValue : IKubernetesObject<V1ObjectMeta>, IMetadata<V1ObjectMeta>
    where TList : IKubernetesObject<V1ListMeta>, IItems<TValue>
{
    protected override Task<IList<TValue>> FetchAllAsync(CancellationToken? cancellationToken = null) =>
        domain.GetResources(cancellationToken);

    protected override async Task<IList<TValue>> FetchByKeyAsync(
        string key,
        CancellationToken? cancellationToken = null
    )
    {
        if (key == CacheUtils.DefaultCacheRefreshKey)
        {
            return await domain.GetResources(cancellationToken);
        }

        return Array.Empty<TValue>();
    }
}
