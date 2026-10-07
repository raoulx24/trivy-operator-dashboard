using System.Text.Json.Serialization;

namespace TrivyOperator.Dashboard.Infrastructure.Trivy.Schema.ReportSchemas.Sboms;

public class ComponentsCr
{
    [JsonPropertyName("bomFormat")]
    public string BomFormat { get; init; } = string.Empty;

    [JsonPropertyName("components")]
    public ComponentCr[]? ChildComponents { get; set; }

    [JsonPropertyName("dependencies")]
    public DependencyCr[]? Dependencies { get; set; }

    [JsonPropertyName("metadata")]
    public MetadataCr? MetadataCr { get; init; }

    [JsonPropertyName("serialNumber")]
    public string? SerialNumber { get; init; }

    [JsonPropertyName("specVersion")]
    public string SpecVersion { get; init; } = string.Empty;

    [JsonPropertyName("version")]
    public int? Version { get; init; }
}
