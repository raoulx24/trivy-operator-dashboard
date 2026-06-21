using System.Text.Json.Serialization;
using TrivyOperator.Dashboard.Infrastructure.K8s.CustomResources;
using TrivyOperator.Dashboard.Infrastructure.Trivy.ReportSchemas.SecurityAssessments;

namespace TrivyOperator.Dashboard.Infrastructure.Trivy.RbacAssessmentReports.Models;

public class RbacAssessmentReportCr : CustomResource
{
    [JsonPropertyName("report")]
    public Report? Report { get; init; }
}
