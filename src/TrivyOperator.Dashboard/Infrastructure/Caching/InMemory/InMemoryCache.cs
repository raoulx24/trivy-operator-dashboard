using System.Collections.Concurrent;
using TrivyOperator.Dashboard.Domain.K8s.ValueObjects;
using TrivyOperator.Dashboard.Domain.Shared.Abstractions;
using TrivyOperator.Dashboard.Infrastructure.Caching.InMemory.Abstractions;
using TrivyOperator.Dashboard.Infrastructure.ResourceStore.Abstractions;

namespace TrivyOperator.Dashboard.Infrastructure.Caching.InMemory;

public class InMemoryCache<TResource, TKey>(
    IResourceConcurrentDictionaryCache<TKey, TResource> cache,
    ILogger<InMemoryCache<TResource, TKey>> logger) : IResourceStore<TResource, TKey>
    where TResource: class, IEntity<TKey>
    where TKey : notnull

{
    public Task Upsert(NamespaceName namespaceName, TResource resource, CancellationToken? ctx = null)
    {
        logger.LogDebug(
            "Upsert - {objectType} - {cacheKey}",
            typeof(TResource).Name,
            namespaceName.ToString()
        );
        
        ConcurrentDictionary<TKey, TResource> innerCache = cache.GetOrAdd(
            namespaceName,
            _ => new ConcurrentDictionary<TKey, TResource>());

        innerCache[resource.Id] = resource;
        
        return Task.CompletedTask;
    }

    public Task Delete(NamespaceName namespaceName, TKey key, CancellationToken? ctx = null)
    {
        logger.LogDebug(
            "Delete - {objectType} - {cacheKey}",
            typeof(TResource).Name,
            namespaceName.ToString()
        );

        if (!cache.TryGetValue(
                namespaceName,
                out ConcurrentDictionary<TKey, TResource>? kubernetesObjectsCache
            ))
        {
            return Task.CompletedTask;
        }

        kubernetesObjectsCache.TryRemove(key, out _);

        return Task.CompletedTask;
    }

    public Task<TResource?> Get(NamespaceName namespaceName, TKey key, CancellationToken? ctx = null)
    {
        logger.LogDebug(
            "Get - {objectType} - {cacheKey}",
            typeof(TResource).Name,
            namespaceName.ToString()
        );
        
        if (!cache.TryGetValue(namespaceName, out ConcurrentDictionary<TKey, TResource>? innerCache))
        {
            return Task.FromResult<TResource?>(null);
        }

        return innerCache.TryGetValue(key, out TResource? resource)
            ? Task.FromResult<TResource?>(resource) 
            : Task.FromResult<TResource?>(null);
    }

    public Task ClearByNamespace(NamespaceName namespaceName, CancellationToken? ctx = null)
    {
        logger.LogDebug(
            "ClearByNs - {objectType} - {cacheKey}",
            typeof(TResource).Name,
            namespaceName.ToString()
        );
        cache.TryRemove(namespaceName, out _);
        return Task.CompletedTask;
    }
}
