using System.Text.Json.Serialization;
using TrivyOperator.Dashboard.Domain.TrivyOld.CustomResources.Abstractions;

namespace TrivyOperator.Dashboard.Domain.TrivyOld.ClusterSbomReport;

public class ClusterSbomReportCr : CustomResource
{
    [JsonPropertyName("report")]
    public Report? Report { get; init; }
}
