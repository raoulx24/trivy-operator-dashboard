using System.Text.Json.Serialization;
using TrivyOperator.Dashboard.Domain.TrivyOld.CustomResources.Abstractions;

namespace TrivyOperator.Dashboard.Domain.TrivyOld.ClusterInfraAssessmentReport;

public class ClusterInfraAssessmentReportCr : CustomResource
{
    [JsonPropertyName("report")]
    public Report? Report { get; init; }
}
