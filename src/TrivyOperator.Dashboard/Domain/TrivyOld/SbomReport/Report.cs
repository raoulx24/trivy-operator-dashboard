using System.Text.Json.Serialization;

namespace TrivyOperator.Dashboard.Domain.Trivy.SbomReport;

public class Report
{
    [JsonPropertyName("artifact")]
    public Artifact Artifact { get; init; } = new();

    [JsonPropertyName("components")]
    public Components Components { get; init; } = new();

    [JsonPropertyName("registry")]
    public Registry Registry { get; init; } = new();

    [JsonPropertyName("scanner")]
    public Scanner Scanner { get; init; } = new();

    [JsonPropertyName("summary")]
    public Summary Summary { get; init; } = new();

    [JsonPropertyName("updateTimestamp")]
    public DateTime? UpdateTimestamp { get; init; }
}
