using k8s;
using k8s.Models;
using TrivyOperator.Dashboard.Domain.K8s.Abstractions;
using TrivyOperator.Dashboard.Infrastructure.Caching.Abstractions;
using TrivyOperator.Dashboard.Infrastructure.Utils;

namespace TrivyOperator.Dashboard.Infrastructure.Caching;

public class ClusterResourcePassthroughCache<TValue, TList>(
    IClusterScopedResourceWatchDomainService<TValue, TList> domain
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
