using System.Text.Json.Serialization;

namespace TrivyOperator.Dashboard.Infrastructure.Trivy.Schema.ReportSchemas.Sboms;

public class CdxComponents
{
    [JsonPropertyName("bomFormat")]
    public string BomFormat { get; init; } = string.Empty;

    [JsonPropertyName("components")]
    public CdxComponent[] ChildComponents { get; set; } = [];

    [JsonPropertyName("dependencies")]
    public CdxDependency[] Dependencies { get; set; } = [];

    [JsonPropertyName("metadata")]
    public CdxMetadata CdxMetadata { get; init; } = new();

    [JsonPropertyName("serialNumber")]
    public string SerialNumber { get; init; } = string.Empty;

    [JsonPropertyName("specVersion")]
    public string SpecVersion { get; init; } = string.Empty;

    [JsonPropertyName("version")]
    public long Version { get; init; }
}
