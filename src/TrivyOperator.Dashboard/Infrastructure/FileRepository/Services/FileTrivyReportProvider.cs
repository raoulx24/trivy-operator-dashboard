using System.Collections.Concurrent;
using TrivyOperator.Dashboard.Domain.K8s.ValueObjects;
using TrivyOperator.Dashboard.Domain.Shared.Stores.Abstractions;
using TrivyOperator.Dashboard.Domain.Trivy.Entities.Abstracts;
using TrivyOperator.Dashboard.Infrastructure.Caching.ConcurrentCache.Abstractions;
using TrivyOperator.Dashboard.Infrastructure.Caching.InMemory.CacheEntries;
using TrivyOperator.Dashboard.Infrastructure.FileRepository.Services.Abstractions;
using TrivyOperator.Dashboard.Infrastructure.Persistence.K8s.Builders.Abstractions;

namespace TrivyOperator.Dashboard.Infrastructure.FileRepository.Services;

public class FileTrivyReportProvider<TTrivyReport, TKey>(
    IExpiringResourceConcurrentDictionaryCache<TKey, CacheEntry<TTrivyReport, TKey>> cache,
    ICacheEntryBuilder<TTrivyReport, TKey> cacheEntryBuilder,
    IFileTrivyReportService<TTrivyReport, TKey> reportService,
    ILogger<FileTrivyReportProvider<TTrivyReport, TKey>> logger
) : IResourceProvider<TTrivyReport, TKey>
    where TTrivyReport : class, ITrivyReport<TKey>
    where TKey : notnull
{
    private readonly SemaphoreSlim refreshLock = new(1, 1);
    private static readonly ContextName DefaultContext = new();

    public async Task<TTrivyReport?> GetResource(TKey key, CancellationToken ctx = default)
    {
        await EnsureCacheLoaded(ctx);

        CacheEntry<TTrivyReport, TKey>? cacheEntry = cache[DefaultContext].GetValueOrDefault(key);
        
        return cacheEntry is null ? null : cacheEntryBuilder.ToEntity(cacheEntry);
    }

    public async Task<IReadOnlyList<TTrivyReport>> GetResources(CancellationToken ctx = default)
    {
        await EnsureCacheLoaded(ctx);

        return
        [
            .. cache[DefaultContext]
                .Values
                .Select(cacheEntryBuilder.ToEntity),
        ];
    }

    public async Task<IReadOnlyList<TTrivyReport>> GetResources(IEnumerable<TKey> keys, CancellationToken ctx = default)
    {
        await EnsureCacheLoaded(ctx);

        ConcurrentDictionary<TKey, CacheEntry<TTrivyReport, TKey>> reports = cache[DefaultContext];
        List<TTrivyReport> result = [];

        foreach (TKey key in keys)
        {
            ctx.ThrowIfCancellationRequested();

            if (reports.TryGetValue(key, out CacheEntry<TTrivyReport, TKey>? entry))
            {
                result.Add(cacheEntryBuilder.ToEntity(entry));
            }
        }

        return result;
    }

    public async Task<TTrivyReport?> GetResourceSummary(TKey key, CancellationToken ctx = default)
    {
        await EnsureCacheLoaded(ctx);

        CacheEntry<TTrivyReport, TKey>? cacheEntry = cache[DefaultContext].GetValueOrDefault(key);
        
        return cacheEntry?.Entry;
    }

    public async Task<IReadOnlyList<TTrivyReport>> GetResourceSummaries(CancellationToken ctx = default)
    {
        await EnsureCacheLoaded(ctx);

        return
        [
            .. cache[DefaultContext]
                .Values
                .Select(x => x.Entry),
        ];
    }

    public async Task<IReadOnlyList<TKey>> GetResourceIds(CancellationToken ctx = default)
    {
        await EnsureCacheLoaded(ctx);

        return [.. cache[DefaultContext].Keys,];
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

    private async Task EnsureCacheLoaded(CancellationToken ctx)
    {
        if (!cache.IsStale())
        {
            return;
        }

        await refreshLock.WaitAsync(ctx);

        try
        {
            if (!cache.IsStale())
            {
                return;
            }

            logger.LogInformation("Refreshing Trivy report cache");

            IReadOnlyDictionary<TKey, TTrivyReport> aggregated = await reportService.GetReportsAsync(ctx);

            ConcurrentDictionary<TKey, CacheEntry<TTrivyReport, TKey>> reports = new(Environment.ProcessorCount, aggregated.Count);

            foreach ((TKey key, TTrivyReport value) in aggregated)
            {
                reports[key] = cacheEntryBuilder.ToCacheEntry(value);
            }


            cache.Clear();

            cache[DefaultContext] = reports;

            logger.LogInformation("Trivy report cache refreshed with {ReportCount} reports", reports.Count);
        }
        finally
        {
            refreshLock.Release();
        }
    }
}
