using System.Collections.Concurrent;
using TrivyOperator.Dashboard.Domain.K8s.ValueObjects;
using TrivyOperator.Dashboard.Domain.Shared.Abstractions;
using TrivyOperator.Dashboard.Infrastructure.Caching.InMemory.Abstractions;
using TrivyOperator.Dashboard.Infrastructure.ResourceStore.Abstractions;

namespace TrivyOperator.Dashboard.Infrastructure.Caching.InMemory;

public class InMemoryCache<TResource, TKey>(
    IResourceConcurrentDictionaryCache<TKey, TResource> cache,
    ILogger<InMemoryCache<TResource, TKey>> logger) : IResourceStore<TResource>
    where TResource: IEntity<TKey>
    where TKey : notnull

{
    public Task UpsertResource(NamespaceName namespaceName, TResource resource)
    {
        logger.LogDebug(
            "Upsert - {objectType} - {cacheKey}",
            typeof(TResource).Name,
            namespaceName.ToString()
        );
        
        if (cache.TryGetValue(
                namespaceName,
                out ConcurrentDictionary<TKey, TResource>? kubernetesObjectsCache
            ))
        {
            kubernetesObjectsCache[resource.Id] = resource;
        }
        else // first time, the cache is really empty
        {
            cache.TryAdd(
                namespaceName,
                new ConcurrentDictionary<TKey, TResource>
                {
                    [resource.Id] = resource,
                }
            );
        }
        
        return Task.CompletedTask;
    }

    public Task DeleteResource(NamespaceName namespaceName, TResource resource)
    {
        return Task.CompletedTask;
    }
}
