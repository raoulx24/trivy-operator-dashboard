using System.Text.Json.Serialization;
using TrivyOperator.Dashboard.Domain.Trivy.CustomResources.Abstractions;

namespace TrivyOperator.Dashboard.Domain.Trivy.ConfigAuditReport;

public class ConfigAuditReportCr : CustomResource
{
    [JsonPropertyName("report")]
    public Report? Report { get; init; }
}
