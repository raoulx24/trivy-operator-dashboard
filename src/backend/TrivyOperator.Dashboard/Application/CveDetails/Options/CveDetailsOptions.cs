namespace TrivyOperator.Dashboard.Application.CveDetails.Options;

// on demand, per CVE (only CVE subjects) - only successful lookups are cached, until the whole cache expires
// (InMemoryCache:ExpireInMinutes without any read)
// Provider - where the details come from: CveOrg (default) or Nvd; read at startup for the composition
// AllowAnyCve - any CVE can be looked up, not only the CVE subjects: the dashboard becomes a proxy to the provider (and
// spends its rate limit on whatever is asked); the lookups then need no vulnerability reports. Read at startup for the
// composition
public class CveDetailsOptions
{
    public bool Enabled { get; init; } = true;
    public string Provider { get; init; } = CveDetailsProviders.CveOrg;
    public bool AllowAnyCve { get; init; }
}

public static class CveDetailsProviders
{
    public const string CveOrg = "CveOrg";
    public const string Nvd = "Nvd";
}
