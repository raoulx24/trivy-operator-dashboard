using System.Text.Json.Serialization;
using TrivyOperator.Dashboard.Domain.TrivyOld.CustomResources.Abstractions;

namespace TrivyOperator.Dashboard.Domain.TrivyOld.ClusterComplianceReport;

public class ClusterComplianceReportCr : CustomResource
{
    [JsonPropertyName("spec")]
    public Spec Spec { get; init; } = new();

    [JsonPropertyName("status")]
    public Status Status { get; init; } = new();
}
