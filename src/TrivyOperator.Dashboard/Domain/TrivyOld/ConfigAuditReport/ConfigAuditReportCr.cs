using System.Text.Json.Serialization;
using TrivyOperator.Dashboard.Domain.TrivyOld.CustomResources.Abstractions;

namespace TrivyOperator.Dashboard.Domain.TrivyOld.ConfigAuditReport;

public class ConfigAuditReportCr : CustomResource
{
    [JsonPropertyName("report")]
    public Report? Report { get; init; }
}
