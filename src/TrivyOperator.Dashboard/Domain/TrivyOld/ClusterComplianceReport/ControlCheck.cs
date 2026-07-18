using System.Text.Json.Serialization;
using TrivyOperator.Dashboard.Domain.Trivy;

namespace TrivyOperator.Dashboard.Domain.TrivyOld.ClusterComplianceReport;

public class ControlCheck
{
    [JsonPropertyName("id")]
    public string Id { get; init; } = string.Empty;

    [JsonPropertyName("name")]
    public string Name { get; init; } = string.Empty;

    [JsonPropertyName("severity")]
    public TrivySeverity Severity { get; init; }

    [JsonPropertyName("totalFail")]
    public int TotalFail { get; init; } = 0;
}
