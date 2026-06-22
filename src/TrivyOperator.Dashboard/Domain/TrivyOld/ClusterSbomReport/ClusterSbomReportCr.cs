using System.Text.Json.Serialization;
using TrivyOperator.Dashboard.Domain.Trivy.CustomResources.Abstractions;

namespace TrivyOperator.Dashboard.Domain.Trivy.ClusterSbomReport;

public class ClusterSbomReportCr : CustomResource
{
    [JsonPropertyName("report")]
    public Report? Report { get; init; }
}
