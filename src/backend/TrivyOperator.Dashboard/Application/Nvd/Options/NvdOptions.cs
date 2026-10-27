namespace TrivyOperator.Dashboard.Application.Nvd.Options;

// on demand, per CVE (only CVE subjects) - only successful lookups are cached, until the whole cache expires
// (InMemoryCache:ExpireInMinutes without any read)
// the NVD rate limit is applied locally, failing fast: 5 requests per 30s without an api key, 50 with one
// ApiKey - optional; request one at https://nvd.nist.gov/developers/request-an-api-key
public class NvdOptions
{
    public bool Enabled { get; init; } = true;
    public string BaseUrl { get; init; } = "https://services.nvd.nist.gov/rest/json/cves/2.0";
    public string? ApiKey { get; init; }

    public int RequestTimeoutInSeconds { get; init; } = 30;
}
