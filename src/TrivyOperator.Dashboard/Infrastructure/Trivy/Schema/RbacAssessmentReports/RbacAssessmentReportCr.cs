using System.Text.Json.Serialization;
using TrivyOperator.Dashboard.Infrastructure.K8s.CustomResources;
using TrivyOperator.Dashboard.Infrastructure.Trivy.Schema.Abstracts;
using TrivyOperator.Dashboard.Infrastructure.Trivy.Schema.ReportSchemas.SecurityAssessments;

namespace TrivyOperator.Dashboard.Infrastructure.Trivy.Schema.RbacAssessmentReports;

public class RbacAssessmentReportCr : CustomResource, ISecurityAssessmentReportCr
{
    [JsonPropertyName("report")]
    public ReportCr Report { get; init; } = new();
}
