using System.Collections.Concurrent;
using TrivyOperator.Dashboard.Domain.K8s.ValueObjects;
using TrivyOperator.Dashboard.Domain.Shared.Stores.Abstractions;
using TrivyOperator.Dashboard.Domain.Trivy.Entities.Abstracts;
using TrivyOperator.Dashboard.Infrastructure.Caching.ConcurrentCache.Abstractions;
using TrivyOperator.Dashboard.Infrastructure.Caching.InMemory.CacheEntries;
using TrivyOperator.Dashboard.Infrastructure.K8s.Contexts.Abstractions;
using TrivyOperator.Dashboard.Infrastructure.K8s.CustomResources;
using TrivyOperator.Dashboard.Infrastructure.K8s.Services.Abstractions;
using TrivyOperator.Dashboard.Infrastructure.Persistence.Aggregators.Abstracts;
using TrivyOperator.Dashboard.Infrastructure.Persistence.Trivy.Builders.Abstractions;

namespace TrivyOperator.Dashboard.Infrastructure.K8s.Providers;

public class KubernetesResourceProvider<TKubernetesObject, TReport, TKey>(
    IExpiringResourceConcurrentDictionaryCache<TKey, CacheEntry<TReport, TKey>> cache,
    ICacheEntryBuilder<TReport, TKey> cacheEntryBuilder,
    IKubernetesResourceService<TKubernetesObject> resourceService,
    IKubernetesContextResolver contextResolver,
    ITrivyReportAggregator<TKubernetesObject, TReport, TKey> aggregator,
    ILogger<KubernetesResourceProvider<TKubernetesObject, TReport, TKey>> logger
)
    : IResourceProvider<TReport, TKey>
    where TKubernetesObject : CustomResource
    where TReport : class, ITrivyReport<TKey>
    where TKey : notnull
{
    // one refresh at a time by design. if multiple Kubernetes contexts are used concurrently,
    // this may be replaced with per-context locks
    private readonly SemaphoreSlim refreshLock = new(1, 1);

    public async Task<TReport?> GetResource(TKey key, CancellationToken ctx = default)
    {
        _ = contextResolver.TryResolveCurrentContext(out ContextName contextName);
        await EnsureCacheLoaded(contextName, ctx);

        CacheEntry<TReport, TKey>? cacheEntry = cache[contextName].GetValueOrDefault(key);
        
        return cacheEntry is null ? null : cacheEntryBuilder.ToEntity(cacheEntry);
    }

    public async Task<IReadOnlyList<TReport>> GetResources(CancellationToken ctx = default)
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

    public async Task<IReadOnlyList<TReport>> GetResourceSummaries(CancellationToken ctx = default)
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
                "Refreshing Kubernetes Trivy report cache for context {Context}",
                context);

            IList<TKubernetesObject> resources = await resourceService.GetResources(ctx);

            IReadOnlyDictionary<TKey, TReport> aggregated = aggregator.Aggregate(resources, ctx);

            ConcurrentDictionary<TKey, CacheEntry<TReport, TKey>> reports = new(Environment.ProcessorCount, aggregated.Count);

            foreach ((TKey key, TReport value) in aggregated)
            {
                reports[key] = cacheEntryBuilder.ToCacheEntry(value);
            }

            cache[context] = reports;

            logger.LogInformation(
                "Kubernetes Trivy report cache refreshed with {ReportCount} reports for context {Context}",
                reports.Count,
                context);
        }
        finally
        {
            refreshLock.Release();
        }
    }
}
