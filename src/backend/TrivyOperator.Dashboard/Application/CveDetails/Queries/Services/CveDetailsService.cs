using System.Collections.Concurrent;
using TrivyOperator.Dashboard.Application.CveDetails.Abstractions;
using TrivyOperator.Dashboard.Application.CveDetails.Queries.Models;
using TrivyOperator.Dashboard.Application.CveDetails.Queries.Services.Abstractions;
using TrivyOperator.Dashboard.Application.Shared.Cache.Abstractions;
using TrivyOperator.Dashboard.Domain.CveDetails.Entities;
using TrivyOperator.Dashboard.Domain.Shared.ValueObjects;
using TrivyOperator.Dashboard.Application.Shared.Queries;
using TrivyOperator.Dashboard.Application.Shared.Queries.Services;

namespace TrivyOperator.Dashboard.Application.CveDetails.Queries.Services;

// - only the subjects (the CVEs of the vulnerability reports of the current context) - not an open proxy to the
//   provider; without subjects (CveDetails:AllowAnyCve) any CVE is looked up
// - only Ok is cached, until the whole cache expires; NotListed and Error are not, so asking again retries
// - concurrent requests for the same CVE share one provider call
// singleton - it holds the in-flight calls
public class CveDetailsService(
    ICveDetailsSubjects? subjects,
    ICache<CveId, CveSourceResult<CveDetailsInfo>> cache,
    ICveDetailsProvider provider) : QueryService, ICveDetailsService
{
    private readonly ConcurrentDictionary<CveId, Lazy<Task<CveSourceResult<CveDetailsInfo>>>> inFlight = new();

    public async Task<CveDetailsResult> GetById(string cveId, CancellationToken ctx = default)
    {
        CveId id = new(cveId);

        if (!id.IsValid)
        {
            return CveDetailsResult.Of(CveDetailsLookupOutcomes.InvalidId);
        }

        if (subjects is not null && !await subjects.Contains(id, ctx))
        {
            return CveDetailsResult.Of(CveDetailsLookupOutcomes.NotSubject);
        }

        if (cache.TryGetValue(id, out CveSourceResult<CveDetailsInfo>? cached))
        {
            return CveDetailsResult.Found(cached.ToCveDetailsDto(id));
        }

        Lazy<Task<CveSourceResult<CveDetailsInfo>>> call =
            inFlight.GetOrAdd(id, key => new Lazy<Task<CveSourceResult<CveDetailsInfo>>>(() => Fetch(key)));

        // the caller may give up (ctx); the shared call goes on for the others (bounded by the client timeout)
        CveSourceResult<CveDetailsInfo> result = await call.Value.WaitAsync(ctx);

        return result.Status == CveSourceStatuses.NotListed
            ? CveDetailsResult.Of(CveDetailsLookupOutcomes.NotListed)
            : CveDetailsResult.Found(result.ToCveDetailsDto(id));
    }

    private async Task<CveSourceResult<CveDetailsInfo>> Fetch(CveId id)
    {
        try
        {
            CveSourceResult<CveDetailsInfo> result = await provider.GetCve(id, CancellationToken.None);

            if (result.Status == CveSourceStatuses.Ok)
            {
                cache[id] = result;
            }

            return result;
        }
        finally
        {
            // only this call can be in flight for the id - a new one is added only after this removal
            inFlight.TryRemove(id, out _);
        }
    }
}
