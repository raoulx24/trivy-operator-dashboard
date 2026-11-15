using TrivyOperator.Dashboard.Domain.CveDetails.Entities;
using TrivyOperator.Dashboard.Domain.Shared.ValueObjects;

namespace TrivyOperator.Dashboard.Application.CveDetails.Abstractions;

public interface ICveDetailsProvider
{
    // one CVE from the configured provider (CVE.org CVE Services API, or NVD CVE API 2.0)
    // Ok - found; NotListed - unknown to the provider; Error - failed or rate limited (the message says when to retry)
    // url, api key and timeout come from the provider's options - the api key is a secret, so it is not passed around
    Task<CveSourceResult<CveDetailsInfo>> GetCve(CveId id, CancellationToken ctx = default);
}
