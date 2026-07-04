using System.Text.Json.Serialization;

namespace TrivyOperator.Dashboard.Infrastructure.Trivy.Schema.ReportSchemas.ClusterComplianceReports;

public class SpecCr
{
    [JsonPropertyName("compliance")]
    public ComplianceCr ComplianceCr { get; init; } = new();

    [JsonPropertyName("cron")]
    public string Cron { get; init; } = string.Empty;

    [JsonPropertyName("reportType")]
    public string ReportType { get; init; } = string.Empty;
}
