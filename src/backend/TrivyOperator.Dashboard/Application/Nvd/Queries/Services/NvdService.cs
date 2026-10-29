using System.Collections.Concurrent;
using TrivyOperator.Dashboard.Application.Nvd.Abstractions;
using TrivyOperator.Dashboard.Application.Nvd.Queries.Models;
using TrivyOperator.Dashboard.Application.Nvd.Queries.Services.Abstractions;
using TrivyOperator.Dashboard.Application.Shared.Cache.Abstractions;
using TrivyOperator.Dashboard.Domain.Nvd.Entities;
using TrivyOperator.Dashboard.Domain.Shared.ValueObjects;

namespace TrivyOperator.Dashboard.Application.Nvd.Queries.Services;

// - only CVE subjects (the CVEs of the current vulnerability reports) - the dashboard is not an open proxy to NVD
// - only Ok is cached, until the whole cache expires; NotListed and Error are not, so asking again retries
// - concurrent requests for the same CVE share one NVD call
// singleton - it holds the in-flight calls
public class NvdService(
    ICache<CveId, Timestamp> subjects,
    ICache<CveId, CveSourceResult<NvdInfo>> cache,
    INvdProvider provider) : INvdService
{
    private readonly ConcurrentDictionary<CveId, Lazy<Task<CveSourceResult<NvdInfo>>>> inFlight = new();

    public bool IsEnabled => true;

    public async Task<NvdCveResult> GetById(string cveId, CancellationToken ctx = default)
    {
        CveId id = new(cveId);

        if (!id.IsValid)
        {
            return NvdCveResult.Of(NvdLookupOutcomes.InvalidId);
        }

        if (!subjects.ContainsKey(id))
        {
            return NvdCveResult.Of(NvdLookupOutcomes.NotSubject);
        }

        if (cache.TryGetValue(id, out CveSourceResult<NvdInfo>? cached))
        {
            return NvdCveResult.Found(cached.ToNvdCveDto(id));
        }

        Lazy<Task<CveSourceResult<NvdInfo>>> call =
            inFlight.GetOrAdd(id, key => new Lazy<Task<CveSourceResult<NvdInfo>>>(() => Fetch(key)));

        // the caller may give up (ctx); the shared call goes on for the others (bounded by the client timeout)
        CveSourceResult<NvdInfo> result = await call.Value.WaitAsync(ctx);

        return result.Status == CveSourceStatuses.NotListed
            ? NvdCveResult.Of(NvdLookupOutcomes.NotInNvd)
            : NvdCveResult.Found(result.ToNvdCveDto(id));
    }

    private async Task<CveSourceResult<NvdInfo>> Fetch(CveId id)
    {
        try
        {
            CveSourceResult<NvdInfo> result = await provider.GetCve(id, CancellationToken.None);

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
