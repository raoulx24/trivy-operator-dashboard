using System.Text.Json.Serialization;

namespace TrivyOperator.Dashboard.Infrastructure.CveEnrichment.Models;

// CISA KEV catalog - known_exploited_vulnerabilities.json
// see https://www.cisa.gov/sites/default/files/feeds/known_exploited_vulnerabilities_schema.json
// only the fields used by the enrichment are mapped
public class KevCatalogResponse
{
    [JsonPropertyName("catalogVersion")]
    public string? CatalogVersion { get; init; }

    [JsonPropertyName("count")]
    public int? Count { get; init; }

    [JsonPropertyName("vulnerabilities")]
    public KevVulnerability[]? Vulnerabilities { get; init; }
}

public class KevVulnerability
{
    [JsonPropertyName("cveID")]
    public string? CveId { get; init; }

    [JsonPropertyName("dateAdded")]
    public DateOnly? DateAdded { get; init; }

    [JsonPropertyName("dueDate")]
    public DateOnly? DueDate { get; init; }

    // "Known" or "Unknown"
    [JsonPropertyName("knownRansomwareCampaignUse")]
    public string? KnownRansomwareCampaignUse { get; init; }

    [JsonPropertyName("requiredAction")]
    public string? RequiredAction { get; init; }
}
