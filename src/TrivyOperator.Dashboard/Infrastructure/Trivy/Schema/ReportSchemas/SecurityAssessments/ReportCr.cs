using System.Text.Json.Serialization;
using TrivyOperator.Dashboard.Infrastructure.Trivy.Schema.ReportSchemas.Shared;

namespace TrivyOperator.Dashboard.Infrastructure.Trivy.Schema.ReportSchemas.SecurityAssessments;

public class ReportCr
{
    [JsonPropertyName("checks")]
    public SecurityAssessmentCheckCr[] Checks { get; init; } = [];

    [JsonPropertyName("scanner")]
    public ScannerCr? Scanner { get; init; }

    [JsonPropertyName("summary")]
    public SummaryCr? Summary { get; init; }

    [JsonPropertyName("updateTimestamp")]
    public DateTime? UpdateTimestamp { get; init; }
}
