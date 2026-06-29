using System.Text.Json.Serialization;

namespace TrivyOperator.Dashboard.Infrastructure.Trivy.Schema.ReportSchemas.Sboms;

public class ComponentCr
{
    [JsonPropertyName("bom-ref")]
    public string BomRef { get; set; } = string.Empty;

    [JsonPropertyName("licenses")]
    public LicenseContainerCr[]? Licenses { get; init; }

    [JsonPropertyName("name")]
    public string Name { get; init; } = string.Empty;

    [JsonPropertyName("properties")]
    public PropertyCr[] Properties { get; init; } = [];

    [JsonPropertyName("purl")]
    public string Purl { get; init; } = string.Empty;

    [JsonPropertyName("supplier")]
    public SupplierCr? Supplier { get; init; }

    [JsonPropertyName("type")]
    public string Type { get; init; } = string.Empty;

    [JsonPropertyName("version")]
    public string Version { get; init; } = string.Empty;
}
