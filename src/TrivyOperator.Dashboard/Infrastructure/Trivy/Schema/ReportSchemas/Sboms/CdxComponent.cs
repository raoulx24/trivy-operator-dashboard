using System.Text.Json.Serialization;

namespace TrivyOperator.Dashboard.Infrastructure.Trivy.Schema.ReportSchemas.Sboms;

public class CdxComponent
{
    [JsonPropertyName("bom-ref")]
    public string BomRef { get; set; } = string.Empty;

    [JsonPropertyName("licenses")]
    public CdxLicenseContainer[]? Licenses { get; init; }

    [JsonPropertyName("name")]
    public string Name { get; init; } = string.Empty;

    [JsonPropertyName("properties")]
    public CdxProperty[] Properties { get; init; } = [];

    [JsonPropertyName("purl")]
    public string Purl { get; init; } = string.Empty;

    [JsonPropertyName("supplier")]
    public CdxSupplier? Supplier { get; init; }

    [JsonPropertyName("type")]
    public string Type { get; init; } = string.Empty;

    [JsonPropertyName("version")]
    public string Version { get; init; } = string.Empty;
}
