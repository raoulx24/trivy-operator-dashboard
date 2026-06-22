using System.Text.Json.Serialization;

namespace TrivyOperator.Dashboard.Domain.Trivy.RbacAssessmentReport;

public class Report
{
    [JsonPropertyName("checks")]
    public Check[] Checks { get; set; } = [];

    [JsonPropertyName("scanner")]
    public Scanner Scanner { get; set; } = new();

    [JsonPropertyName("summary")]
    public Summary Summary { get; set; } = new();
}
