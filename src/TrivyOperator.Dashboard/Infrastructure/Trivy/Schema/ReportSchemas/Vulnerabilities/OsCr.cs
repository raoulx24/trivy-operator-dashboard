using System.Text.Json.Serialization;

namespace TrivyOperator.Dashboard.Infrastructure.Trivy.Schema.ReportSchemas.Vulnerabilities;

public class OsCr
{
    [JsonPropertyName("eosl")]
    public bool? Eosl { get; init; }
    
    [JsonPropertyName("family")]
    public string? Family { get; init; }

    [JsonPropertyName("name")]
    public string? Name { get; init; }
}
