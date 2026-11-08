using TrivyOperator.Dashboard.Domain.Shared.Details;
using TrivyOperator.Dashboard.Domain.Trivy.ValueObjects.Sboms;
using TrivyOperator.Dashboard.Domain.Trivy.ValueObjects.Shared;
using TrivyOperator.Dashboard.Infrastructure.Caching.ConcurrentCache.Abstractions;

namespace TrivyOperator.Dashboard.Infrastructure.Caching.InMemory.Details.Sboms;

// the PurlDetails in use: the purls of the ImagePurls store
// (an image removed by the housekeeping frees its purls on a later housekeeping run)
public class InMemoryPurlDetailReferenceProvider(
    IResourceDictionaryCache<Digest, DetailEntry<ImagePurls>> cache
) : InMemoryDetailEntryReferenceProvider<ImagePurls, Digest, Purl>(cache)
{
    protected override IEnumerable<Purl> GetIds(ImagePurls imagePurls) => imagePurls.Purls;
}
