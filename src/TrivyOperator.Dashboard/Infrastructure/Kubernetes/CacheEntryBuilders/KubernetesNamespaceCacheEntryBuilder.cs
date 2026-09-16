using TrivyOperator.Dashboard.Domain.Kubernetes.Entities;
using TrivyOperator.Dashboard.Domain.Kubernetes.ValueObjects;
using TrivyOperator.Dashboard.Infrastructure.Caching.InMemory.CacheEntries;
using TrivyOperator.Dashboard.Infrastructure.Kubernetes.CacheEntryBuilders.Abstractions;

namespace TrivyOperator.Dashboard.Infrastructure.Kubernetes.CacheEntryBuilders;

public class KubernetesNamespaceCacheEntryBuilder
    : ICacheEntryBuilder<KubernetesNamespace, Uid>
{
    public CacheEntry<KubernetesNamespace, Uid> ToCacheEntry(KubernetesNamespace entry)
    {
        return new CacheEntry<KubernetesNamespace, Uid>
        {
            Entry = entry,
            EncodedDetails = [],
        };
    }

    public KubernetesNamespace ToEntity(CacheEntry<KubernetesNamespace, Uid> cacheEntry)
    {
        return cacheEntry.Entry;
    }
}
