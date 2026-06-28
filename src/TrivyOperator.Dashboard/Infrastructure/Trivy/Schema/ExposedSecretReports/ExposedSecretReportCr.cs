using System.Text.Json.Serialization;
using TrivyOperator.Dashboard.Infrastructure.Trivy.Schema.ReportSchemas.ExposedSecrets;

namespace TrivyOperator.Dashboard.Infrastructure.Trivy.Schema.ExposedSecretReports;

public class ExposedSecretReportCr
{
    [JsonPropertyName("report")]
    public Report? Report { get; init; }
}
