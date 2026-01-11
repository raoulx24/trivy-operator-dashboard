using k8s;
using k8s.Models;
using TrivyOperator.Dashboard.Domain.K8s.Abstractions;
using TrivyOperator.Dashboard.Infrastructure.Caching.Abstractions;
using TrivyOperator.Dashboard.Infrastructure.Utils;

namespace TrivyOperator.Dashboard.Infrastructure.Caching;

public class NamespacedResourcePassthroughCache<TValue, TList>(
    INamespacedResourceWatchDomainService<TValue, TList> domain)
    : ResourcePassthroughCache<TValue>
    where TValue : IKubernetesObject<V1ObjectMeta>, IMetadata<V1ObjectMeta>
    where TList : IKubernetesObject<V1ListMeta>, IItems<TValue>
{
    protected override Task<IList<TValue>> FetchAllAsync(CancellationToken? cancellationToken = null)
        => domain.GetResources(cancellationToken);

    protected override Task<IList<TValue>> FetchByKeyAsync(
        string key,
        CancellationToken? cancellationToken = null)
        => key == CacheUtils.DefaultCacheRefreshKey
            ? domain.GetResources(cancellationToken)
            : domain.GetResources(key, cancellationToken);
}
