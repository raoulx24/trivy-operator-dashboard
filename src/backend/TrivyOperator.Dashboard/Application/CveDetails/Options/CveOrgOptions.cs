namespace TrivyOperator.Dashboard.Application.CveDetails.Options;

// settings of the CVE.org provider (CveDetails:Provider = CveOrg), section CveDetails:CveOrg - the CVE Services API,
// read only, no api key
// the service has its own rate limit (25000 requests per minute, sent back in the RateLimit-* headers), so none is
// applied locally
public class CveOrgOptions
{
    public string BaseUrl { get; init; } = "https://cveawg.mitre.org/api/cve";

    public int RequestTimeoutInSeconds { get; init; } = 30;
}
