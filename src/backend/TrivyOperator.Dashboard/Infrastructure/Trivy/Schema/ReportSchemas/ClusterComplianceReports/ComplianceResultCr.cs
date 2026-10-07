using System.Text.Json.Serialization;
using TrivyOperator.Dashboard.Infrastructure.Trivy.Schema.ReportSchemas.Shared;

namespace TrivyOperator.Dashboard.Infrastructure.Trivy.Schema.ReportSchemas.ClusterComplianceReports;

public class ComplianceResultCr
{
    [JsonPropertyName("checks")]
    public SecurityAssessmentCheckCr[] Checks { get; init; } = [];

    [JsonPropertyName("description")]
    public string? Description { get; init; }

    [JsonPropertyName("id")]
    public string? Id { get; init; }

    [JsonPropertyName("name")]
    public string? Name { get; init; }

    [JsonPropertyName("severity")]
    public string? Severity { get; init; }

    [JsonPropertyName("status")]
    public string? Status { get; init; }
}
