using System.Text.Json.Serialization;
using TrivyOperator.Dashboard.Infrastructure.Kubernetes.CustomResources;
using TrivyOperator.Dashboard.Infrastructure.Trivy.Schema.Abstracts;
using TrivyOperator.Dashboard.Infrastructure.Trivy.Schema.ReportSchemas.SecurityAssessments;

namespace TrivyOperator.Dashboard.Infrastructure.Trivy.Schema.RbacAssessmentReports;

public class ClusterRbacAssessmentReportCr : CustomResource, ISecurityAssessmentReportCr
{
    [JsonPropertyName("report")]
    public ReportCr Report { get; init; } = new();
}
