using TrivyOperator.Dashboard.Domain.Nvd.Entities;
using TrivyOperator.Dashboard.Domain.Shared.ValueObjects;

namespace TrivyOperator.Dashboard.Application.Nvd.Abstractions;

public interface INvdProvider
{
    // one CVE from the NVD CVE API 2.0
    // Ok - found; NotListed - unknown to NVD; Error - failed or rate limited (the message says when to retry)
    // url, api key and timeout come from the options - the api key is a secret, so it is not passed around
    Task<CveSourceResult<NvdInfo>> GetCve(CveId id, CancellationToken ctx = default);
}
