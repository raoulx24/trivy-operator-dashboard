using System.Text.Json.Serialization;
using TrivyOperator.Dashboard.Infrastructure.K8s.CustomResources;
using TrivyOperator.Dashboard.Infrastructure.Trivy.Schema.ReportSchemas.Sboms;

namespace TrivyOperator.Dashboard.Infrastructure.Trivy.Schema.SbomReports;

public class ClusterSbomReportCr : CustomResource
{
    [JsonPropertyName("report")]
    public ReportCr? Report { get; init; }
}
