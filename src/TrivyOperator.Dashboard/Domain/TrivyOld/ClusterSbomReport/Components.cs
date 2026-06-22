using System.Text.Json.Serialization;

namespace TrivyOperator.Dashboard.Domain.Trivy.ClusterSbomReport;

public class Components
{
    [JsonPropertyName("bomFormat")]
    public string BomFormat { get; init; } = string.Empty;

    [JsonPropertyName("components")]
    public ComponentsComponent[] ComponentsComponents { get; init; } = [];

    [JsonPropertyName("dependencies")]
    public Dependency[] Dependencies { get; init; } = [];

    [JsonPropertyName("metadata")]
    public ComponentsMetadata Metadata { get; init; } = new();

    [JsonPropertyName("serialNumber")]
    public string SerialNumber { get; init; } = string.Empty;

    [JsonPropertyName("specVersion")]
    public string SpecVersion { get; init; } = string.Empty;

    [JsonPropertyName("version")]
    public long Version { get; init; }
}
