using System.Text.Json.Serialization;

namespace TrivyOperator.Dashboard.Infrastructure.CveEnrichment.Models;

// ENISA EUVD api - GET /search?exploited=true&page=..&size=..
// only the fields used by the enrichment are mapped
public class EuvdSearchResponse
{
    [JsonPropertyName("items")]
    public EuvdItem[]? Items { get; init; }

    [JsonPropertyName("total")]
    public int? Total { get; init; }
}

public class EuvdItem
{
    [JsonPropertyName("id")]
    public string? Id { get; init; }

    // new line separated (e.g. "GHSA-jfh8-c2jp-5v3q\nCVE-2021-44228\n")
    [JsonPropertyName("aliases")]
    public string? Aliases { get; init; }

    // e.g. "Dec 10, 2021, 12:00:00 AM", no time zone
    [JsonPropertyName("exploitedSince")]
    public string? ExploitedSince { get; init; }

    // percent, 0..100
    [JsonPropertyName("epss")]
    public double? Epss { get; init; }
}
