using System.Text.Json.Serialization;

namespace TrivyOperator.Dashboard.Domain.Trivy.ClusterRbacAssessmentReport;

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
