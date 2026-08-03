using System.Collections.Concurrent;
using TrivyOperator.Dashboard.Domain.K8s.Abstractions;
using TrivyOperator.Dashboard.Domain.K8s.ValueObjects;
using TrivyOperator.Dashboard.Domain.Shared.Abstractions;
using TrivyOperator.Dashboard.Infrastructure.Caching.ConcurrentCache.Abstractions;
using TrivyOperator.Dashboard.Infrastructure.Caching.InMemory.CacheEntries;
using TrivyOperator.Dashboard.Infrastructure.Persistence.Trivy.Builders.Abstractions;

namespace TrivyOperator.Dashboard.Infrastructure.Caching.InMemory;

public abstract class InMemoryEntityCache<TResource, TKey>(
    IResourceConcurrentDictionaryCache<TKey, CacheEntry<TResource, TKey>> cache,
    ICacheEntryBuilder<TResource, TKey> cacheEntryBuilder,
    ILogger<InMemoryEntityCache<TResource, TKey>> logger) :
    IResourceRepository<TResource, TKey>
    where TResource: class, IEntity<TKey>
    where TKey : notnull

{
    protected IResourceConcurrentDictionaryCache<TKey, CacheEntry<TResource, TKey>> Cache  => cache;
    
    public Task Upsert(ContextName namespaceName, TResource resource, CancellationToken ctx = default)
    {
        logger.LogDebug(
            "Upsert - {objectType} - {cacheKey}",
            typeof(TResource).Name,
            namespaceName.ToString()
        );
        
        ctx.ThrowIfCancellationRequested();
        
        ConcurrentDictionary<TKey, CacheEntry<TResource, TKey>> innerCache = cache.GetOrAdd(
            namespaceName,
            _ => new ConcurrentDictionary<TKey, CacheEntry<TResource, TKey>>());

        innerCache[resource.Id] = cacheEntryBuilder.ToCacheEntry(resource);
        
        return Task.CompletedTask;
    }

    public Task<TResource?> Get(ContextName contextName, TKey key, CancellationToken ctx = default)
    {
        logger.LogDebug(
            "Get - {objectType} - {cacheKey}",
            typeof(TResource).Name,
            contextName.ToString()
        );
        
        ctx.ThrowIfCancellationRequested();
        
        if (!cache.TryGetValue(contextName, out ConcurrentDictionary<TKey, CacheEntry<TResource, TKey>>? innerCache))
        {
            return Task.FromResult<TResource?>(null);
        }

        return innerCache.TryGetValue(key, out CacheEntry<TResource, TKey>? cacheEntry)
            ? Task.FromResult<TResource?>(cacheEntryBuilder.ToEntity(cacheEntry)) 
            : Task.FromResult<TResource?>(null);
    }
    
    // public Task<TResource?> GetLight(ContextName contextName, TKey key, CancellationToken ctx = default)
    // {
    //     logger.LogDebug(
    //         "Get - {objectType} - {cacheKey}",
    //         typeof(TResource).Name,
    //         contextName.ToString()
    //     );
    //     
    //     ctx.ThrowIfCancellationRequested();
    //     
    //     if (!cache.TryGetValue(contextName, out ConcurrentDictionary<TKey, CacheEntry<TResource, TKey>>? innerCache))
    //     {
    //         return Task.FromResult<TResource?>(null);
    //     }
    //
    //     return innerCache.TryGetValue(key, out CacheEntry<TResource, TKey>? cacheEntry)
    //         ? Task.FromResult<TResource?>(cacheEntry.Entry) 
    //         : Task.FromResult<TResource?>(null);
    // }

    public abstract Task ClearByNamespace(ContextName contextName, NamespaceName ns, CancellationToken ctx = default);
    
    public abstract Task Delete(ContextName contextName, TKey key, Uid uid, CancellationToken ctx = default);

    public Task<IReadOnlyList<TResource>> GetResources(ContextName contextName = default, CancellationToken ctx = default)
    {
        ctx.ThrowIfCancellationRequested();
        
        if (contextName == default) contextName = new ContextName();
        
        if (!cache.TryGetValue(contextName, out ConcurrentDictionary<TKey, CacheEntry<TResource, TKey>>? innerCache))
        {
            return Task.FromResult<IReadOnlyList<TResource>>([]);
        }

        IReadOnlyList<TResource> result = [.. innerCache.Values.Select(cacheEntryBuilder.ToEntity),];
        
        return Task.FromResult(result);
    }
}
