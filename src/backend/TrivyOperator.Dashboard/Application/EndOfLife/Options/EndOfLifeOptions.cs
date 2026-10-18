namespace TrivyOperator.Dashboard.Application.EndOfLife.Options;

public class EndOfLifeOptions
{
    public bool Enabled { get; init; } = true;
    public string BaseUrl { get; init; } = "https://endoflife.date/api/v1";

    public int ProcessingIntervalInSeconds { get; init; } = 60;
    public int ProductTtlInMinutes { get; init; } = 1440;
    public int NotFoundTtlInMinutes { get; init; } = 1440;
    public int ErrorTtlInMinutes { get; init; } = 15;
    public int RequestTimeoutInSeconds { get; init; } = 30;

    // optional json file with an EndOfLife:ProductMappings section, merged over the shipped endoflife.config.json
    // both are added to configuration with reload on change
    public string? ProductMappingsFile { get; init; }

    // Trivy OS family -> endoflife.date product id; defaults are shipped in endoflife.config.json
    public Dictionary<string, string> ProductMappings { get; init; } = new(StringComparer.OrdinalIgnoreCase);
}
