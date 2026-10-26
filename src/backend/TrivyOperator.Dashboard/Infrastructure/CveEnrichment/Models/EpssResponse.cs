using System.Text.Json.Serialization;

namespace TrivyOperator.Dashboard.Infrastructure.CveEnrichment.Models;

// FIRST EPSS api - GET /epss?cve=...
// see https://www.first.org/epss/api
// epss and percentile are sent as strings (e.g. "0.003320000")
public class EpssResponse
{
    [JsonPropertyName("status")]
    public string? Status { get; init; }

    [JsonPropertyName("total")]
    public int? Total { get; init; }

    [JsonPropertyName("data")]
    public EpssData[]? Data { get; init; }
}

public class EpssData
{
    [JsonPropertyName("cve")]
    public string? Cve { get; init; }

    [JsonPropertyName("epss")]
    public string? Epss { get; init; }

    [JsonPropertyName("percentile")]
    public string? Percentile { get; init; }

    [JsonPropertyName("date")]
    public DateOnly? Date { get; init; }
}
