namespace TrivyOperator.Dashboard.Infrastructure.Caching.InMemory.Abstractions;

public interface
    IExpiringResourceConcurrentDictionaryCache<TKey, TValue> : IResourceConcurrentDictionaryCache<TKey, TValue>
    where TKey : notnull
{
    bool IsStale();
}