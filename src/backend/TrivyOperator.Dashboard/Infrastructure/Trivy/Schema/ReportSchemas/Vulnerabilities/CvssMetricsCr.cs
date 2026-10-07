using System.Text.Json.Serialization;

namespace TrivyOperator.Dashboard.Infrastructure.Trivy.Schema.ReportSchemas.Vulnerabilities;

public class CvssMetricsCr
{
    [JsonPropertyName("V2Score")]
    public double? V2Score { get; init; }

    [JsonPropertyName("V2Vector")]
    public string? V2Vector { get; init; }

    [JsonPropertyName("V3Score")]
    public double? V3Score { get; init; }

    [JsonPropertyName("V3Vector")]
    public string? V3Vector { get; init; }

    [JsonPropertyName("V40Score")]
    public double? V40Score { get; init; }

    [JsonPropertyName("V40Vector")]
    public string? V40Vector { get; init; }
}
