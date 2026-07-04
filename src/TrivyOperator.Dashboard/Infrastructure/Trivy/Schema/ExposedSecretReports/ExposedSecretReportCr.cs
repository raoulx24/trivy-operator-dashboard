using System.Text.Json.Serialization;
using TrivyOperator.Dashboard.Infrastructure.Trivy.Schema.ReportSchemas.ExposedSecrets;

namespace TrivyOperator.Dashboard.Infrastructure.Trivy.Schema.ExposedSecretReports;

public class ExposedSecretReportCr
{
    [JsonPropertyName("report")]
    public ReportCr Report { get; init; } = new();
}
