using System.Text.Json.Serialization;
using TrivyOperator.Dashboard.Infrastructure.Trivy.ReportSchemas.ExposedSecrets;

namespace TrivyOperator.Dashboard.Infrastructure.Trivy.ExposedSecretReports.Models;

public class ExposedSecretReportCr
{
    [JsonPropertyName("report")]
    public Report? Report { get; init; }
}
