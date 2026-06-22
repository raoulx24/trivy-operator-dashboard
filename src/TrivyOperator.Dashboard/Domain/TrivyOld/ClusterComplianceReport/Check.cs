using System.Text.Json.Serialization;

namespace TrivyOperator.Dashboard.Domain.Trivy.ClusterComplianceReport;

public class Check
{
    [JsonPropertyName("id")]
    public string Id { get; init; } = string.Empty;
}
