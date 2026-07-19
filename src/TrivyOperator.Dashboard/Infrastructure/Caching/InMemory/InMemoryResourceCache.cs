using System.Collections.Concurrent;
using TrivyOperator.Dashboard.Domain.K8s.Abstractions;
using TrivyOperator.Dashboard.Domain.K8s.ValueObjects;
using TrivyOperator.Dashboard.Domain.Shared.Abstractions;
using TrivyOperator.Dashboard.Infrastructure.Caching.InMemory.Abstractions;

namespace TrivyOperator.Dashboard.Infrastructure.Caching.InMemory;

public class InMemoryResourceCache<TResource, TKey>(
    IResourceConcurrentDictionaryCache<TKey, TResource> cache,
    ILogger<InMemoryResourceCache<TResource, TKey>> logger) :
    IResourceRepository<TResource, TKey>
    where TResource: class, IEntity<TKey>
    where TKey : notnull

{
    public Task Upsert(NamespaceName namespaceName, TResource resource, CancellationToken ctx = default)
    {
        logger.LogDebug(
            "Upsert - {objectType} - {cacheKey}",
            typeof(TResource).Name,
            namespaceName.ToString()
        );
        
        ctx.ThrowIfCancellationRequested();
        
        ConcurrentDictionary<TKey, TResource> innerCache = cache.GetOrAdd(
            namespaceName,
            _ => new ConcurrentDictionary<TKey, TResource>());

        innerCache[resource.Id] = resource;
        
        return Task.CompletedTask;
    }

    public Task InitResources(NamespaceName namespaceName, IEnumerable<TResource> resources, CancellationToken ctx = default)
    {
        logger.LogDebug(
            "InitResources - {objectType} - {cacheKey}",
            typeof(TResource).Name,
            namespaceName.ToString()
        );
        
        ctx.ThrowIfCancellationRequested();

        ConcurrentDictionary<TKey, TResource> newCache =
            new(resources.ToDictionary(r => r.Id));

        cache[namespaceName] = newCache;

        return Task.CompletedTask;
    }

    public Task Delete(NamespaceName namespaceName, TKey key, CancellationToken ctx = default)
    {
        logger.LogDebug(
            "Delete - {objectType} - {cacheKey}",
            typeof(TResource).Name,
            namespaceName.ToString()
        );
        
        ctx.ThrowIfCancellationRequested();

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

    public Task<TResource?> Get(NamespaceName namespaceName, TKey key, CancellationToken ctx = default)
    {
        logger.LogDebug(
            "Get - {objectType} - {cacheKey}",
            typeof(TResource).Name,
            namespaceName.ToString()
        );
        
        ctx.ThrowIfCancellationRequested();
        
        if (!cache.TryGetValue(namespaceName, out ConcurrentDictionary<TKey, TResource>? innerCache))
        {
            return Task.FromResult<TResource?>(null);
        }

        return innerCache.TryGetValue(key, out TResource? resource)
            ? Task.FromResult<TResource?>(resource) 
            : Task.FromResult<TResource?>(null);
    }

    public Task ClearByNamespace(NamespaceName namespaceName, CancellationToken ctx = default)
    {
        logger.LogDebug(
            "ClearByNs - {objectType} - {cacheKey}",
            typeof(TResource).Name,
            namespaceName.ToString()
        );
        
        ctx.ThrowIfCancellationRequested();
        
        cache.TryRemove(namespaceName, out _);
        return Task.CompletedTask;
    }

    public Task ClearAll(CancellationToken ctx = default)
    {
        ctx.ThrowIfCancellationRequested();
        
        cache.Clear();

        return Task.CompletedTask;
    }

    public Task<IReadOnlyList<TResource>> GetResources(CancellationToken ctx = default)
    {
        ctx.ThrowIfCancellationRequested();
        
        IReadOnlyList<TResource> result = [.. cache.Values.SelectMany(x => x.Values),];

        return Task.FromResult(result);
    }

    public Task<IReadOnlyList<TResource>> GetResources(
        NamespaceName namespaceName = default,
        CancellationToken ctx = default
    )
    {
        ctx.ThrowIfCancellationRequested();
        
        if (namespaceName == default) namespaceName = new NamespaceName();

        if (!cache.TryGetValue(namespaceName, out ConcurrentDictionary<TKey, TResource>? namespaceCache))
        {
            return Task.FromResult<IReadOnlyList<TResource>>([]);
        }

        IReadOnlyList<TResource> result = [.. namespaceCache.Values,];

        return Task.FromResult(result);
    }
}
