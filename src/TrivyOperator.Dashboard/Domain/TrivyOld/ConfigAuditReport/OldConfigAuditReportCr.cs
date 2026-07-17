using System.Text.Json.Serialization;
using TrivyOperator.Dashboard.Infrastructure.K8s.CustomResources;

namespace TrivyOperator.Dashboard.Domain.TrivyOld.ConfigAuditReport;

public class OldConfigAuditReportCr : CustomResource
{
    [JsonPropertyName("report")]
    public Report? Report { get; init; }
}
