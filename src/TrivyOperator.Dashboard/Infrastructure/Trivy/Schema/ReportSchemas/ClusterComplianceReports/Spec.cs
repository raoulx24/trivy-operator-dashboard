using System.Text.Json.Serialization;

namespace TrivyOperator.Dashboard.Infrastructure.Trivy.Schema.ReportSchemas.ClusterComplianceReports;

public class Spec
{
    [JsonPropertyName("compliance")]
    Compliance Compliance { get; init; } = new();

    [JsonPropertyName("cron")]
    public string Cron { get; init; } = string.Empty;

    [JsonPropertyName("reportType")]
    public string ReportType { get; init; } = string.Empty;
}
