using TrivyOperator.Dashboard.Domain.Shared.Abstractions;
using TrivyOperator.Dashboard.Infrastructure.Caching.InMemory.CacheEntries;
using TrivyOperator.Dashboard.Infrastructure.Kubernetes.CacheEntryBuilders.Abstractions;

namespace TrivyOperator.Dashboard.Infrastructure.Kubernetes.CacheEntryBuilders;

public class PassthroughCacheEntryBuilder<TEntity, TId> : ICacheEntryBuilder<TEntity, TId>
    where TEntity : IEntity<TId>
{
    public CacheEntry<TEntity, TId> ToCacheEntry(TEntity entry)
    {
        return new CacheEntry<TEntity, TId>
        {
            Entry = entry,
            EncodedDetails = [],
        };
    }

    public TEntity ToEntity(CacheEntry<TEntity, TId> cacheEntry)
    {
        return cacheEntry.Entry;
    }
}
