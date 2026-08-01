using TrivyOperator.Dashboard.Domain.Shared.Abstractions;

namespace TrivyOperator.Dashboard.Infrastructure.Caching.InMemory.CacheEntries.Builders.Abstractions;

public interface ICacheEntryBuilder<TEntity, TId>
where TEntity : IEntity<TId>
{
    CacheEntry<TEntity, TId> ToCacheEntry(TEntity entry);
    TEntity ToEntity(CacheEntry<TEntity, TId> cacheEntry);
}
