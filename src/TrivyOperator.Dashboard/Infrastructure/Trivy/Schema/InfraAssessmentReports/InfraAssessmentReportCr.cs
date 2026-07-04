using System.Text.Json.Serialization;
using TrivyOperator.Dashboard.Infrastructure.K8s.CustomResources;
using TrivyOperator.Dashboard.Infrastructure.Trivy.Schema.ReportSchemas.SecurityAssessments;

namespace TrivyOperator.Dashboard.Infrastructure.Trivy.Schema.InfraAssessmentReports;

public class InfraAssessmentReportCr : CustomResource
{
    [JsonPropertyName("report")]
    public ReportCr Report { get; init; } = new();
}
