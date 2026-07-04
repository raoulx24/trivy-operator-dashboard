using System.Text.Json.Serialization;

namespace TrivyOperator.Dashboard.Infrastructure.Trivy.Schema.ReportSchemas.Sboms;

public class ComponentCr
{
    [JsonPropertyName("bom-ref")]
    public string? BomRef { get; init; }

    [JsonPropertyName("group")]
    public string? Group { get; init; }
    
    [JsonPropertyName("hashes")]
    public HashCr[]? Hashes { get; init; }

    [JsonPropertyName("licenses")]
    public LicenseContainerCr[]? Licenses { get; init; }

    [JsonPropertyName("name")]
    public string? Name { get; init; }

    [JsonPropertyName("properties")]
    public PropertyCr[]? Properties { get; init; }

    [JsonPropertyName("purl")]
    public string? Purl { get; init; }

    [JsonPropertyName("supplier")]
    public SupplierCr? Supplier { get; init; }

    [JsonPropertyName("type")]
    public string? Type { get; init; }

    [JsonPropertyName("version")]
    public string? Version { get; init; }
}
