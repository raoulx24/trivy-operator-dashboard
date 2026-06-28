using System.Text.Json.Serialization;
using TrivyOperator.Dashboard.Infrastructure.Trivy.Schema.ReportSchemas.Shared;

namespace TrivyOperator.Dashboard.Infrastructure.Trivy.Schema.ReportSchemas.Sboms;

public class Report
{
    [JsonPropertyName("artifact")]
    public Artifact Artifact { get; init; } = new();

    [JsonPropertyName("components")]
    public CdxComponents CdxComponents { get; init; } = new();

    [JsonPropertyName("registry")]
    public Registry Registry { get; init; } = new();

    [JsonPropertyName("scanner")]
    public Scanner Scanner { get; init; } = new();

    [JsonPropertyName("summary")]
    public CdxSummary CdxSummary { get; init; } = new();

    [JsonPropertyName("updateTimestamp")]
    public DateTime? UpdateTimestamp { get; init; }
}
