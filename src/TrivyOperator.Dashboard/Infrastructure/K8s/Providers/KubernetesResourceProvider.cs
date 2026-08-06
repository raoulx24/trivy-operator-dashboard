using System.Collections.Concurrent;
using TrivyOperator.Dashboard.Domain.K8s.Abstractions;
using TrivyOperator.Dashboard.Domain.K8s.ValueObjects;
using TrivyOperator.Dashboard.Domain.Trivy.Entities.Abstracts;
using TrivyOperator.Dashboard.Infrastructure.Caching.ConcurrentCache.Abstractions;
using TrivyOperator.Dashboard.Infrastructure.K8s.ClientFactory.Abstractions;
using TrivyOperator.Dashboard.Infrastructure.K8s.CustomResources;
using TrivyOperator.Dashboard.Infrastructure.K8s.Services.Abstractions;
using TrivyOperator.Dashboard.Infrastructure.Persistence.Aggregators.Abstracts;

namespace TrivyOperator.Dashboard.Infrastructure.K8s.Providers;

public class KubernetesResourceProvider<TKubernetesObject, TReport, TKey>(
    IExpiringResourceConcurrentDictionaryCache<TKey, TReport> cache,
    IKubernetesResourceService<TKubernetesObject> resourceService,
    IKubernetesContextResolver contextResolver,
    ITrivyReportAggregator<TKubernetesObject, TReport, TKey> aggregator,
    ILogger<KubernetesResourceProvider<TKubernetesObject, TReport, TKey>> logger
)
    : IResourceProvider<TReport>
    where TKubernetesObject : CustomResource
    where TReport : class, ITrivyReport<TKey>
    where TKey : notnull
{
    // one refresh at a time by design. if multiple Kubernetes contexts are used concurrently,
    // this may be replaced with per-context locks
    private readonly SemaphoreSlim refreshLock = new(1, 1);

    public async Task<IReadOnlyList<TReport>> GetResources(CancellationToken ctx = default)
    {
        _ = contextResolver.TryResolveCurrentContext(out ContextName contextName);
        await EnsureCacheLoaded(contextName, ctx);

        return cache[contextName].Values.ToList();
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

            IReadOnlyDictionary<TKey, TReport> reports = aggregator.Aggregate(resources, ctx);

            cache[context] = new ConcurrentDictionary<TKey, TReport>(reports);

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
