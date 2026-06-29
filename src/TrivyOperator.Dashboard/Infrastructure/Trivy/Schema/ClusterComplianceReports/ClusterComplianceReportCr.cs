using System.Text.Json.Serialization;
using TrivyOperator.Dashboard.Infrastructure.K8s.CustomResources;
using TrivyOperator.Dashboard.Infrastructure.Trivy.Schema.ReportSchemas.ClusterComplianceReports;

namespace TrivyOperator.Dashboard.Infrastructure.Trivy.Schema.ClusterComplianceReports;

public class ClusterComplianceReportCr : CustomResource
{
    [JsonPropertyName("spec")]
    public SpecCr SpecCr { get; init; } = new();

    [JsonPropertyName("status")]
    public StatusCr StatusCr { get; init; } = new();
}
