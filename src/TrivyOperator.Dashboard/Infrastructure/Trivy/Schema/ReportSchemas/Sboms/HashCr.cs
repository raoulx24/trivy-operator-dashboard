using System.Text.Json.Serialization;

namespace TrivyOperator.Dashboard.Infrastructure.Trivy.Schema.ReportSchemas.Sboms;

public class HashCr
{
    [JsonPropertyName("alg")]
    public string? Alg { get; init; }

    [JsonPropertyName("content")]
    public string? Content { get; init; }
}
