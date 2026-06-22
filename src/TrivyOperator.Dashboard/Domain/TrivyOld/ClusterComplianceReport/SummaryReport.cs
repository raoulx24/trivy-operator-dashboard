using System.Text.Json.Serialization;

namespace TrivyOperator.Dashboard.Domain.Trivy.ClusterComplianceReport;

public class SummaryReport
{
    [JsonPropertyName("controlCheck")]
    public ControlCheck[] ControlCheck { get; init; } = [];

    [JsonPropertyName("id")]
    public string Id { get; init; } = string.Empty;

    [JsonPropertyName("title")]
    public string Title { get; init; } = string.Empty;
}
