using TrivyOperator.Dashboard.Domain.Shared.Abstractions;
using TrivyOperator.Dashboard.Infrastructure.Caching.InMemory.Abstractions;

namespace TrivyOperator.Dashboard.Infrastructure.Caching.InMemory;

public class ExpiringInMemoryResourceCache<TResource, TKey>(
    IExpiringResourceConcurrentDictionaryCache<TKey, TResource> cache,
    ILogger<InMemoryResourceCache<TResource, TKey>> logger) :
    InMemoryResourceCache<TResource, TKey>(cache, logger)
    where TResource: class, IEntity<TKey>
    where TKey : notnull
{
    public bool IsStale => cache.IsStale();
}