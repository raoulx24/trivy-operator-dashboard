using System.Collections.Concurrent;
using TrivyOperator.Dashboard.Application.Trivy.SecurityAssessmentDetails.Abstractions;
using TrivyOperator.Dashboard.Domain.Kubernetes.ValueObjects;
using TrivyOperator.Dashboard.Domain.Trivy.Entities.Abstracts;
using TrivyOperator.Dashboard.Domain.Trivy.ValueObjects.SecurityAssessments;
using TrivyOperator.Dashboard.Infrastructure.Caching.ConcurrentCache.Abstractions;
using TrivyOperator.Dashboard.Infrastructure.Caching.InMemory.CacheEntries;

namespace TrivyOperator.Dashboard.Infrastructure.Caching.InMemory;

public class InMemoryCheckReferenceProvider<TSlim, TReport>(
    IResourceDictionaryCache<Uid, CacheEntry<TSlim, Uid>> cache
) : IDetailReferenceProvider<CheckId>
    where TSlim : class, ISecurityAssessmentSlimReport<TSlim, TReport, Uid>
{
    public Task<IReadOnlyDictionary<ContextName, IReadOnlySet<CheckId>>> GetReferencedIds(
        CancellationToken ctx = default)
    {
        Dictionary<ContextName, IReadOnlySet<CheckId>> result = [];

        foreach (KeyValuePair<ContextName, ConcurrentDictionary<Uid, CacheEntry<TSlim, Uid>>> context in cache.ToArray())
        {
            ctx.ThrowIfCancellationRequested();

            HashSet<CheckId> ids = [];

            foreach (CacheEntry<TSlim, Uid> entry in context.Value.Values)
            {
                ids.UnionWith(entry.Entry.CheckIds);
            }

            result[context.Key] = ids;
        }

        return Task.FromResult<IReadOnlyDictionary<ContextName, IReadOnlySet<CheckId>>>(result);
    }
}
