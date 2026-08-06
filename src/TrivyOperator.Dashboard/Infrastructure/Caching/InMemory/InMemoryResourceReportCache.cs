using System.Collections.Concurrent;
using TrivyOperator.Dashboard.Domain.K8s.ValueObjects;
using TrivyOperator.Dashboard.Domain.Trivy.Entities.Abstracts;
using TrivyOperator.Dashboard.Infrastructure.Caching.ConcurrentCache.Abstractions;
using TrivyOperator.Dashboard.Infrastructure.Caching.InMemory.CacheEntries;
using TrivyOperator.Dashboard.Infrastructure.K8s.ClientFactory.Abstractions;
using TrivyOperator.Dashboard.Infrastructure.Persistence.Trivy.Builders.Abstractions;

namespace TrivyOperator.Dashboard.Infrastructure.Caching.InMemory;

public class InMemoryResourceReportCache<TResource>(
    IResourceConcurrentDictionaryCache<Uid, CacheEntry<TResource, Uid>> cache,
    ICacheEntryBuilder<TResource, Uid> cacheEntryBuilder,
    IKubernetesContextResolver contextResolver,
    ILogger<InMemoryResourceReportCache<TResource>> logger)
    : InMemoryEntityCache<TResource, Uid>(cache, cacheEntryBuilder, contextResolver, logger)
    where TResource: class, IResourceReport
{
    public override Task ClearByNamespace(NamespaceName ns, CancellationToken ctx = default)
    {
        _ = ContextResolver.TryResolveCurrentContext(out ContextName contextName);
        
        logger.LogDebug(
            "ClearByNamespace - {objectType} - {cacheKey} - {namespace}",
            typeof(TResource).Name,
            contextName,
            ns);

        ctx.ThrowIfCancellationRequested();

        if (!Cache.TryGetValue(
                contextName,
                out ConcurrentDictionary<Uid, CacheEntry<TResource, Uid>>? innerCache))
        {
            return Task.CompletedTask;
        }

        foreach ((Uid key, CacheEntry<TResource, Uid> entry) in innerCache)
        {
            ctx.ThrowIfCancellationRequested();

            if (entry.Entry.Metadata.NamespaceName == ns)
            {
                innerCache.TryRemove(key, out _);
            }
        }

        return Task.CompletedTask;
    }
    
    public override Task Delete(Uid key, Uid uid, CancellationToken ctx = default)
    {
        _ = ContextResolver.TryResolveCurrentContext(out ContextName contextName);
        
        logger.LogDebug(
            "Delete - {objectType} - {cacheKey} - {uid}",
            typeof(TResource).Name,
            key,
            uid);

        ctx.ThrowIfCancellationRequested();

        if (!Cache.TryGetValue(
                contextName,
                out ConcurrentDictionary<Uid, CacheEntry<TResource, Uid>>? innerCache))
        {
            return Task.CompletedTask;
        }

        innerCache.TryRemove(key, out _);

        return Task.CompletedTask;
    }
}
