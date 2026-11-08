using TrivyOperator.Dashboard.Domain.Trivy.Entities.Abstracts;
using TrivyOperator.Dashboard.Domain.Trivy.ValueObjects.Shared;
using TrivyOperator.Dashboard.Infrastructure.Caching.ConcurrentCache.Abstractions;
using TrivyOperator.Dashboard.Infrastructure.Caching.InMemory.CacheEntries;

namespace TrivyOperator.Dashboard.Infrastructure.Caching.InMemory.Details.Sboms;

// the ImagePurls in use: one per image digest still in the sbom report cache
// (the collector does nothing on delete, so an image is removed from the purl index only by the housekeeping)
public class InMemoryImagePurlsReferenceProvider<TReport>(
    IResourceDictionaryCache<Digest, CacheEntry<TReport, Digest>> cache
) : InMemoryDetailReferenceProvider<TReport, Digest, Digest>(cache)
    where TReport : class, ISbomReport<TReport, Digest>
{
    protected override IEnumerable<Digest> GetIds(TReport report) => [report.Id];
}
