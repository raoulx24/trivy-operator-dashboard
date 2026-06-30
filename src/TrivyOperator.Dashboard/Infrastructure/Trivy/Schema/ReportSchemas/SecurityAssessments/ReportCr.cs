using System.Text.Json.Serialization;
using TrivyOperator.Dashboard.Infrastructure.Trivy.Schema.ReportSchemas.Shared;

namespace TrivyOperator.Dashboard.Infrastructure.Trivy.Schema.ReportSchemas.SecurityAssessments;

public class ReportCr
{
    [JsonPropertyName("checks")]
    public CheckCr[] Checks { get; init; } = [];

    [JsonPropertyName("scanner")]
    public ScannerCr Scanner { get; init; } = new();

    [JsonPropertyName("summary")]
    public SummaryCr Summary { get; init; } = new();

    [JsonPropertyName("updateTimestamp")]
    public DateTime UpdateTimestamp { get; init; } = DateTime.UtcNow;
}
