using System.Text.Json.Serialization;

namespace TrivyOperator.Dashboard.Infrastructure.Trivy.Schema.ReportSchemas.ClusterComplianceReports;

public class Check
{
    [JsonPropertyName("id")]
    public string Id { get; init; } = string.Empty;
}
