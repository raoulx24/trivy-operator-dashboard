using System.Text.Json.Serialization;

namespace TrivyOperator.Dashboard.Infrastructure.Trivy.Schema.ReportSchemas.Shared;

public class ScannerCr
{
    [JsonPropertyName("name")]
    public string Name { get; init; } = string.Empty;

    [JsonPropertyName("vendor")]
    public string Vendor { get; init; } = string.Empty;

    [JsonPropertyName("version")]
    public string Version { get; init; } = string.Empty;
}
