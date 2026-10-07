namespace TrivyOperator.Dashboard.Infrastructure.Caching.ConcurrentCache.Abstractions;

public interface
    IExpiringResourceDictionaryCache<TKey, TValue> : IResourceDictionaryCache<TKey, TValue>
    where TKey : notnull
{
    bool IsStale();

    void ClearIfStale();
}
