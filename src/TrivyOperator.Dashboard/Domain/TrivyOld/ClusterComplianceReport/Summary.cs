using System.Text.Json.Serialization;

namespace TrivyOperator.Dashboard.Domain.Trivy.ClusterComplianceReport;

public class Summary
{
    [JsonPropertyName("failCount")]
    public long FailCount { get; init; } = 0;

    [JsonPropertyName("passCount")]
    public long PassCount { get; init; } = 0;
}
