using System.Text.Json.Serialization;

namespace TrivyOperator.Dashboard.Domain.Trivy.ClusterComplianceReport;

public class Spec
{
    [JsonPropertyName("compliance")]
    public Compliance Compliance { get; init; } = new();

    [JsonPropertyName("cron")]
    public string Cron { get; init; } = string.Empty;

    [JsonPropertyName("reportType")]
    public string ReportType { get; init; } = string.Empty;
}
