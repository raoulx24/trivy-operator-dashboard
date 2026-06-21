using System.Text.Json.Serialization;
using TrivyOperator.Dashboard.Infrastructure.Trivy.ReportSchemas.Shared;

namespace TrivyOperator.Dashboard.Infrastructure.Trivy.ReportSchemas.SecurityAssessments;

public class Report
{
    [JsonPropertyName("checks")]
    public Check[] Checks { get; init; } = [];

    [JsonPropertyName("scanner")]
    public Scanner? Scanner { get; init; }

    [JsonPropertyName("summary")]
    public Summary? Summary { get; init; }

    [JsonPropertyName("updateTimestamp")]
    public DateTime? UpdateTimestamp { get; init; }
}
