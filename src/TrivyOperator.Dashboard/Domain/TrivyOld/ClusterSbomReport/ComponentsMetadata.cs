using System.Text.Json.Serialization;

namespace TrivyOperator.Dashboard.Domain.Trivy.ClusterSbomReport;

public class ComponentsMetadata
{
    [JsonPropertyName("component")]
    public ComponentsComponent Component { get; init; } = new();

    [JsonPropertyName("timestamp")]
    public DateTime? Timestamp { get; init; }

    [JsonPropertyName("tools")]
    public Tools Tools { get; init; } = new();
}
