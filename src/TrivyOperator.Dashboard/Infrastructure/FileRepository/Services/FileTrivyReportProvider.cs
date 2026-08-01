using System.Collections.Concurrent;
using TrivyOperator.Dashboard.Domain.K8s.Abstractions;
using TrivyOperator.Dashboard.Domain.K8s.ValueObjects;
using TrivyOperator.Dashboard.Domain.Trivy.Entities.Abstracts;
using TrivyOperator.Dashboard.Infrastructure.Caching.ConcurrentCache.Abstractions;
using TrivyOperator.Dashboard.Infrastructure.FileRepository.Services.Abstractions;
using TrivyOperator.Dashboard.Infrastructure.Trivy.Mappers.Abstract;

namespace TrivyOperator.Dashboard.Infrastructure.FileRepository.Services;

public class FileTrivyReportProvider<TTrivyReport, TKey>(
    IExpiringResourceConcurrentDictionaryCache<TKey, TTrivyReport> cache,
    IFileTrivyReportService<TTrivyReport> reportService,
    ITrivyReportKeyProvider<TTrivyReport, TKey> keyProvider,
    ILogger<FileTrivyReportProvider<TTrivyReport, TKey>> logger
) : IResourceProvider<TTrivyReport>
where TTrivyReport : ITrivyReport
where TKey : notnull
{
    private readonly SemaphoreSlim refreshLock = new(1, 1);

    public async Task<IReadOnlyList<TTrivyReport>> GetResources(ContextName context,
        CancellationToken ctx = default)
    {
        await EnsureCacheLoaded(ctx);

        List<TTrivyReport> result = [];

        foreach (ConcurrentDictionary<TKey, TTrivyReport> namespaceResources in cache.Values)
        {
            result.AddRange(namespaceResources.Values);
        }

        return result;
    }

    public async Task<IReadOnlyList<TTrivyReport>> GetResources(
        NamespaceName namespaceName = default,
        CancellationToken ctx = default)
    {
        await EnsureCacheLoaded(ctx);

        if (namespaceName.Value == null)
        {
            namespaceName = new NamespaceName();
        }

        if (!cache.TryGetValue(namespaceName, out ConcurrentDictionary<TKey, TTrivyReport>? resources))
        {
            return [];
        }

        return [.. resources.Values,];
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
            // Double check after acquiring the lock.
            if (!cache.IsStale())
            {
                return;
            }

            logger.LogInformation("Refreshing Trivy report cache");

            IReadOnlyDictionary<NamespaceName, IReadOnlyCollection<TTrivyReport>> reports =
                await reportService.GetReportsByNamespaceAsync(ctx);

            cache.Clear();

            foreach ((NamespaceName namespaceName, IReadOnlyCollection<TTrivyReport> namespaceReports) in reports)
            {
                ConcurrentDictionary<TKey, TTrivyReport> namespaceCache = [];

                foreach (TTrivyReport report in namespaceReports)
                {
                    namespaceCache[keyProvider.GetKey(report)] = report;
                }

                cache[namespaceName] = namespaceCache;
            }

            logger.LogInformation(
                "Trivy report cache refreshed with {NamespaceCount} namespaces",
                reports.Count);
        }
        finally
        {
            refreshLock.Release();
        }
    }
}
