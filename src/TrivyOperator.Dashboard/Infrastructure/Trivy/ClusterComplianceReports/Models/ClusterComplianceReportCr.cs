using System.Text.Json.Serialization;
using TrivyOperator.Dashboard.Infrastructure.K8s.CustomResources;
using TrivyOperator.Dashboard.Infrastructure.Trivy.ReportSchemas.ClusterComplianceReports;

namespace TrivyOperator.Dashboard.Infrastructure.Trivy.ClusterComplianceReports.Models;

public class ClusterComplianceReportCr : CustomResource
{
    [JsonPropertyName("spec")]
    public Spec Spec { get; init; } = new();

    [JsonPropertyName("status")]
    public Status Status { get; init; } = new();
}
