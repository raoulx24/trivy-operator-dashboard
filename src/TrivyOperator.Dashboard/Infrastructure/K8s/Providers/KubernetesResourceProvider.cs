using k8s;
using k8s.Models;
using System.Collections.Concurrent;
using TrivyOperator.Dashboard.Domain.K8s.ValueObjects;
using TrivyOperator.Dashboard.Domain.Shared.Abstractions;
using TrivyOperator.Dashboard.Domain.Shared.Stores.Abstractions;
using TrivyOperator.Dashboard.Infrastructure.Caching.ConcurrentCache.Abstractions;
using TrivyOperator.Dashboard.Infrastructure.Caching.InMemory.CacheEntries;
using TrivyOperator.Dashboard.Infrastructure.K8s.Contexts.Abstractions;
using TrivyOperator.Dashboard.Infrastructure.K8s.Services.Abstractions;
using TrivyOperator.Dashboard.Infrastructure.Persistence.Aggregators.Abstracts;
using TrivyOperator.Dashboard.Infrastructure.Persistence.K8s.Builders.Abstractions;

namespace TrivyOperator.Dashboard.Infrastructure.K8s.Providers;

public class KubernetesResourceProvider<TKubernetesObject, TResource, TKey>(
    IExpiringResourceConcurrentDictionaryCache<TKey, CacheEntry<TResource, TKey>> cache,
    ICacheEntryBuilder<TResource, TKey> cacheEntryBuilder,
    IKubernetesResourceService<TKubernetesObject> resourceService,
    IKubernetesContextResolver contextResolver,
    IResourceAggregator<TKubernetesObject, TResource, TKey> aggregator,
    ILogger<KubernetesResourceProvider<TKubernetesObject, TResource, TKey>> logger
)
    : IExpiringResourceProvider<TResource, TKey>
    where TKubernetesObject : IKubernetesObject<V1ObjectMeta>
    where TResource : class, IEntity<TKey>
    where TKey : notnull
{
    // one refresh at a time by design. if multiple Kubernetes contexts are used concurrently,
    // this may be replaced with per-context locks
    private readonly SemaphoreSlim refreshLock = new(1, 1);

    public async Task<TResource?> GetResource(TKey key, CancellationToken ctx = default)
    {
        _ = contextResolver.TryResolveCurrentContext(out ContextName contextName);
        await EnsureCacheLoaded(contextName, ctx);

        CacheEntry<TResource, TKey>? cacheEntry = cache[contextName].GetValueOrDefault(key);
        
        return cacheEntry is null ? null : cacheEntryBuilder.ToEntity(cacheEntry);
    }

    public async Task<IReadOnlyList<TResource>> GetResources(CancellationToken ctx = default)
    {
        _ = contextResolver.TryResolveCurrentContext(out ContextName contextName);
        await EnsureCacheLoaded(contextName, ctx);

        return
        [
            .. cache[contextName]
                .Values
                .Select(cacheEntryBuilder.ToEntity),
        ];
    }

    public async Task<IReadOnlyList<TResource>> GetResources(IEnumerable<TKey> keys, CancellationToken ctx = default)
    {
        _ = contextResolver.TryResolveCurrentContext(out ContextName contextName);
        await EnsureCacheLoaded(contextName, ctx);

        ConcurrentDictionary<TKey, CacheEntry<TResource, TKey>> reports = cache[contextName];
        List<TResource> result = [];

        foreach (TKey key in keys)
        {
            ctx.ThrowIfCancellationRequested();

            if (reports.TryGetValue(key, out CacheEntry<TResource, TKey>? entry))
            {
                result.Add(cacheEntryBuilder.ToEntity(entry));
            }
        }

        return result;
    }

    public async Task<TResource?> GetResourceSummary(TKey key, CancellationToken ctx = default)
    {
        _ = contextResolver.TryResolveCurrentContext(out ContextName contextName);
        await EnsureCacheLoaded(contextName, ctx);

        CacheEntry<TResource, TKey>? cacheEntry = cache[contextName].GetValueOrDefault(key);
        
        return cacheEntry?.Entry;
    }

    public async Task<IReadOnlyList<TResource>> GetResourceSummaries(CancellationToken ctx = default)
    {
        _ = contextResolver.TryResolveCurrentContext(out ContextName contextName);
        await EnsureCacheLoaded(contextName, ctx);

        return
        [
            .. cache[contextName]
                .Values
                .Select(x => x.Entry),
        ];
    }

    public async Task<IReadOnlyList<TKey>> GetResourceIds(CancellationToken ctx = default)
    {
        _ = contextResolver.TryResolveCurrentContext(out ContextName contextName);
        await EnsureCacheLoaded(contextName, ctx);

        return [.. cache[contextName].Keys,];
    }

    public async Task Clear(CancellationToken ctx = default)
    {
        await refreshLock.WaitAsync(ctx);

        try
        {
            cache.Clear();
        }
        finally
        {
            refreshLock.Release();
        }
    }

    private async Task EnsureCacheLoaded(
        ContextName context,
        CancellationToken ctx)
    {
        cache.ClearIfStale();

        if (cache.ContainsKey(context))
        {
            return;
        }

        await refreshLock.WaitAsync(ctx);

        try
        {
            cache.ClearIfStale();

            if (cache.ContainsKey(context))
            {
                return;
            }

            logger.LogInformation(
                "Refreshing Kubernetes resource cache for context {Context}",
                context);

            IList<TKubernetesObject> resources = await resourceService.GetResources(ctx);

            IReadOnlyDictionary<TKey, TResource> aggregated = aggregator.Aggregate(resources, ctx);

            ConcurrentDictionary<TKey, CacheEntry<TResource, TKey>> reports = new(Environment.ProcessorCount, aggregated.Count);

            foreach ((TKey key, TResource value) in aggregated)
            {
                reports[key] = cacheEntryBuilder.ToCacheEntry(value);
            }

            cache[context] = reports;

            logger.LogInformation(
                "Kubernetes resource cache refreshed with {ReportCount} reports for context {Context}",
                reports.Count,
                context);
        }
        finally
        {
            refreshLock.Release();
        }
    }
}
