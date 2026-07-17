using System.Text.Json.Serialization;
using TrivyOperator.Dashboard.Infrastructure.K8s.CustomResources;

namespace TrivyOperator.Dashboard.Domain.TrivyOld.ClusterComplianceReport;

public class OldClusterComplianceReportCr : CustomResource
{
    [JsonPropertyName("spec")]
    public Spec Spec { get; init; } = new();

    [JsonPropertyName("status")]
    public Status Status { get; init; } = new();
}
