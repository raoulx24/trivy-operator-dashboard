using System.Text.Json.Serialization;

namespace TrivyOperator.Dashboard.Domain.Trivy.ClusterComplianceReport;

public class ControlCheck
{
    [JsonPropertyName("id")]
    public string Id { get; init; } = string.Empty;

    [JsonPropertyName("name")]
    public string Name { get; init; } = string.Empty;

    [JsonPropertyName("severity")]
    public TrivySeverity Severity { get; init; }

    [JsonPropertyName("totalFail")]
    public long TotalFail { get; init; } = 0;
}
