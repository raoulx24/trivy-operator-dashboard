using System.Text.Json.Serialization;

namespace TrivyOperator.Dashboard.Infrastructure.Trivy.ReportSchemas.Sboms;

public class Components
{
    [JsonPropertyName("bomFormat")]
    public string BomFormat { get; init; } = string.Empty;

    [JsonPropertyName("components")]
    public Component[] ChildComponents { get; set; } = [];

    [JsonPropertyName("dependencies")]
    public Dependency[] Dependencies { get; set; } = [];

    [JsonPropertyName("metadata")]
    public Metadata Metadata { get; init; } = new();

    [JsonPropertyName("serialNumber")]
    public string SerialNumber { get; init; } = string.Empty;

    [JsonPropertyName("specVersion")]
    public string SpecVersion { get; init; } = string.Empty;

    [JsonPropertyName("version")]
    public long Version { get; init; }
}
