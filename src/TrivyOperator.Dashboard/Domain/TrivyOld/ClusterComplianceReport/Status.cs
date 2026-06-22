using System.Text.Json.Serialization;

namespace TrivyOperator.Dashboard.Domain.Trivy.ClusterComplianceReport;

public class Status
{
    [JsonPropertyName("summary")]
    public Summary Summary { get; init; } = new();

    [JsonPropertyName("summaryReport")]
    public SummaryReport SummaryReport { get; init; } = new();

    [JsonPropertyName("updateTimestamp")]
    public DateTime? UpdateTimestamp { get; init; }
}
