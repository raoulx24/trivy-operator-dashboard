using TrivyOperator.Dashboard.Domain.Kubernetes.ValueObjects;
using TrivyOperator.Dashboard.Domain.Trivy.Entities.Abstracts;
using TrivyOperator.Dashboard.Domain.Trivy.ValueObjects.SecurityAssessments;
using TrivyOperator.Dashboard.Infrastructure.Caching.ConcurrentCache.Abstractions;
using TrivyOperator.Dashboard.Infrastructure.Caching.InMemory.CacheEntries;
using TrivyOperator.Dashboard.Infrastructure.Caching.InMemory.Details;

namespace TrivyOperator.Dashboard.Infrastructure.Caching.InMemory.Details.SecurityAssessments;

public class InMemoryCheckReferenceProvider<TSlim, TReport>(
    IResourceDictionaryCache<Uid, CacheEntry<TSlim, Uid>> cache
) : InMemoryDetailReferenceProvider<TSlim, Uid, CheckId>(cache)
    where TSlim : class, ISecurityAssessmentSlimReport<TSlim, TReport, Uid>
{
    protected override IEnumerable<CheckId> GetIds(TSlim slim) => slim.CheckIds;
}
