using System.Text.Json.Serialization;
using TrivyOperator.Dashboard.Infrastructure.K8s.CustomResources;
using TrivyOperator.Dashboard.Infrastructure.Trivy.Schema.ReportSchemas.SecurityAssessments;

namespace TrivyOperator.Dashboard.Infrastructure.Trivy.Schema.ConfigAuditReports;

public class ClusterConfigAuditReportCr : CustomResource
{
    [JsonPropertyName("report")]
    public Report? Report { get; init; }
}
