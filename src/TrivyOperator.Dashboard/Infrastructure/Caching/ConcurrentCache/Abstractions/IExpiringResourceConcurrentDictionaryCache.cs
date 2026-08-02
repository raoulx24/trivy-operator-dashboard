namespace TrivyOperator.Dashboard.Infrastructure.Caching.ConcurrentCache.Abstractions;

public interface
    IExpiringResourceConcurrentDictionaryCache<TKey, TValue> : IResourceConcurrentDictionaryCache<TKey, TValue>
    where TKey : notnull
{
    bool IsStale();
}
