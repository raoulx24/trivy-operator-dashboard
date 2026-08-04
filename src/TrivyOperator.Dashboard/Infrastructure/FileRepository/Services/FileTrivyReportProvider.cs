using System.Collections.Concurrent;
using TrivyOperator.Dashboard.Domain.K8s.Abstractions;
using TrivyOperator.Dashboard.Domain.K8s.ValueObjects;
using TrivyOperator.Dashboard.Domain.Trivy.Entities.Abstracts;
using TrivyOperator.Dashboard.Infrastructure.Caching.ConcurrentCache.Abstractions;
using TrivyOperator.Dashboard.Infrastructure.FileRepository.Services.Abstractions;

namespace TrivyOperator.Dashboard.Infrastructure.FileRepository.Services;

public class FileTrivyReportProvider<TTrivyReport, TKey>(
    IExpiringResourceConcurrentDictionaryCache<TKey, TTrivyReport> cache,
    IFileTrivyReportService<TTrivyReport, TKey> reportService,
    ILogger<FileTrivyReportProvider<TTrivyReport, TKey>> logger
) : IResourceProvider<TTrivyReport>
where TTrivyReport : ITrivyReport<TKey>
where TKey : notnull
{
    private readonly SemaphoreSlim refreshLock = new(1, 1);

    public async Task<IReadOnlyList<TTrivyReport>> GetResources(ContextName context = default,
        CancellationToken ctx = default)
    {
        await EnsureCacheLoaded(ctx);

        List<TTrivyReport> result = [];

        foreach (ConcurrentDictionary<TKey, TTrivyReport> resources in cache.Values)
        {
            result.AddRange(resources.Values);
        }

        return result;
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

            IReadOnlyDictionary<TKey, TTrivyReport> reports =
                await reportService.GetReportsAsync(ctx);

            ConcurrentDictionary<TKey, TTrivyReport> contextCache =
                new(reports);

            cache.Clear();

            cache[new ContextName()] = contextCache;

            logger.LogInformation(
                "Trivy report cache refreshed with {ReportCount} reports",
                contextCache.Count);
        }
        finally
        {
            refreshLock.Release();
        }
    }
}
