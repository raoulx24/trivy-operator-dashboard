namespace TrivyOperator.Dashboard.Application.CveEnrichment.Options;

public class CveEnrichmentOptions
{
    public bool Enabled { get; init; } = true;

    // CVEs not seen in any report scanned in the last SweepAgeInHours are dropped; 0 disables the sweep
    // must be larger than the report TTL of trivy-operator; set it to 0 if the report TTL is disabled
    public int SweepAgeInHours { get; init; } = 48;
    public int HousekeepingIntervalInSeconds { get; init; } = 300;

    public KevOptions Kev { get; init; } = new();
    public EpssOptions Epss { get; init; } = new();
    public EuvdOptions Euvd { get; init; } = new();
}

// bulk - the whole catalog is downloaded
public class KevOptions
{
    public bool Enabled { get; init; } = true;
    public string Url { get; init; } =
        "https://www.cisa.gov/sites/default/files/feeds/known_exploited_vulnerabilities.json";

    public int ProcessingIntervalInSeconds { get; init; } = 60;
    public int RefreshIntervalInMinutes { get; init; } = 360;
    public int ErrorTtlInMinutes { get; init; } = 15;
    public int RequestTimeoutInSeconds { get; init; } = 60;
}

// per CVE, in batches
public class EpssOptions
{
    public bool Enabled { get; init; } = true;
    public string BaseUrl { get; init; } = "https://api.first.org/data/v1";

    public int ProcessingIntervalInSeconds { get; init; } = 60;
    public int EntryTtlInMinutes { get; init; } = 1440;
    public int BatchSize { get; init; } = 100;
    public int ErrorTtlInMinutes { get; init; } = 15;
    public int RequestTimeoutInSeconds { get; init; } = 30;
}

// bulk - the exploited vulnerabilities are downloaded page by page
public class EuvdOptions
{
    public bool Enabled { get; init; } = true;
    public string BaseUrl { get; init; } = "https://euvdservices.enisa.europa.eu/api";

    public int ProcessingIntervalInSeconds { get; init; } = 60;
    public int RefreshIntervalInMinutes { get; init; } = 360;
    public int PageSize { get; init; } = 100;
    public int ErrorTtlInMinutes { get; init; } = 15;
    public int RequestTimeoutInSeconds { get; init; } = 30;
}
