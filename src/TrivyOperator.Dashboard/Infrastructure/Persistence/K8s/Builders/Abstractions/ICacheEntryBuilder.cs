using TrivyOperator.Dashboard.Domain.Shared.Abstractions;
using TrivyOperator.Dashboard.Infrastructure.Caching.InMemory.CacheEntries;

namespace TrivyOperator.Dashboard.Infrastructure.Persistence.K8s.Builders.Abstractions;

public interface ICacheEntryBuilder<TEntity, TId>
where TEntity : IEntity<TId>
{
    CacheEntry<TEntity, TId> ToCacheEntry(TEntity entry);
    TEntity ToEntity(CacheEntry<TEntity, TId> cacheEntry);
}
