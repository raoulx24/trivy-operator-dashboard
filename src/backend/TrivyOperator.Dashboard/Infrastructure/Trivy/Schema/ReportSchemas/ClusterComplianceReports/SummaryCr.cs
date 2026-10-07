using System.Text.Json.Serialization;

namespace TrivyOperator.Dashboard.Infrastructure.Trivy.Schema.ReportSchemas.ClusterComplianceReports;

public class SummaryCr
{
    [JsonPropertyName("failCount")]
    public int? FailCount { get; init; }

    [JsonPropertyName("passCount")]
    public int? PassCount { get; init; }
}
