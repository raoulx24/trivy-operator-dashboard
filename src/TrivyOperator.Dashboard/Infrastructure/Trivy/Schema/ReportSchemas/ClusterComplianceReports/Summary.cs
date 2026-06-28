using System.Text.Json.Serialization;

namespace TrivyOperator.Dashboard.Infrastructure.Trivy.Schema.ReportSchemas.ClusterComplianceReports;

public class Summary
{
    [JsonPropertyName("failCount")]
    public long FailCount { get; init; } = 0;

    [JsonPropertyName("passCount")]
    public long PassCount { get; init; } = 0;
}
