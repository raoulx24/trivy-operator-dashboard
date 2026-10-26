using TrivyOperator.Dashboard.Domain.CveEnrichment.ValueObjects;

namespace TrivyOperator.Dashboard.Application.CveEnrichment.Abstractions;

// lets the API wake up the source hosted services and force a refetch, ignoring the TTLs
// unlike end of life there is one hosted service per source, so every source gets (and consumes) its own request
public interface ICveEnrichmentRefreshTrigger
{
    void RequestRebuild();

    // waits until either the timeout elapses or a rebuild is requested
    // returns true if a rebuild was requested (and consumes the request for this source)
    Task<bool> WaitAsync(CveSource source, TimeSpan timeout, CancellationToken ctx = default);
}
