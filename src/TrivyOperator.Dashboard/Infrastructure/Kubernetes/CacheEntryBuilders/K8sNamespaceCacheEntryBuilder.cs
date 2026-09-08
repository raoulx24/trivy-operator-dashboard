using TrivyOperator.Dashboard.Domain.Kubernetes.Entities;
using TrivyOperator.Dashboard.Domain.Kubernetes.ValueObjects;
using TrivyOperator.Dashboard.Infrastructure.Caching.InMemory.CacheEntries;
using TrivyOperator.Dashboard.Infrastructure.Kubernetes.CacheEntryBuilders.Abstractions;

namespace TrivyOperator.Dashboard.Infrastructure.Kubernetes.CacheEntryBuilders;

public class K8sNamespaceCacheEntryBuilder
    : ICacheEntryBuilder<K8sNamespace, Uid>
{
    public CacheEntry<K8sNamespace, Uid> ToCacheEntry(K8sNamespace entry)
    {
        return new CacheEntry<K8sNamespace, Uid>
        {
            Entry = entry,
            EncodedDetails = [],
        };
    }

    public K8sNamespace ToEntity(CacheEntry<K8sNamespace, Uid> cacheEntry)
    {
        return cacheEntry.Entry;
    }
}
