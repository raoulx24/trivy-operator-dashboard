using System.Text.Json.Serialization;

namespace TrivyOperator.Dashboard.Infrastructure.Trivy.Schema.ReportSchemas.Sboms;

public class ToolsComponentCr
{
    [JsonPropertyName("group")]
    public string Group { get; init; } = string.Empty;

    [JsonPropertyName("name")]
    public string Name { get; init; } = string.Empty;

    [JsonPropertyName("supplier")]
    public SupplierCr? Supplier { get; init; }

    [JsonPropertyName("type")]
    public string Type { get; init; } = string.Empty;

    [JsonPropertyName("version")]
    public string Version { get; init; } = string.Empty;
}
