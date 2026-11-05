using TrivyOperator.Dashboard.Domain.Shared.Abstractions;
using TrivyOperator.Dashboard.Infrastructure.Caching.InMemory.CacheEntries;

namespace TrivyOperator.Dashboard.Infrastructure.Kubernetes.CacheEntryBuilders.Abstractions;

// the cache keeps an entry (TEntry) plus the encoded details of an entity, and gives the entity back from them
public interface ICacheEntryBuilder<TEntity, TEntry, TId>
where TEntry : IEntity<TId>
{
    CacheEntry<TEntry, TId> ToCacheEntry(TEntity entry);
    TEntity ToEntity(CacheEntry<TEntry, TId> cacheEntry);
    // the entity without its details, from the entry alone
    TEntity ToSummary(CacheEntry<TEntry, TId> cacheEntry);
}

// the entry is the entity itself (without its details), which is also the summary
public interface ICacheEntryBuilder<TEntity, TId> : ICacheEntryBuilder<TEntity, TEntity, TId>
where TEntity : IEntity<TId>
{
    TEntity ICacheEntryBuilder<TEntity, TEntity, TId>.ToSummary(CacheEntry<TEntity, TId> cacheEntry) => cacheEntry.Entry;
}
