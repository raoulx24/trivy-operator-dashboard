using System.Text.Json.Serialization;

namespace TrivyOperator.Dashboard.Infrastructure.Trivy.Schema.ReportSchemas.ClusterComplianceReports;

public class StatusCr
{
    [JsonPropertyName("summary")]
    public SummaryCr? SummaryCr { get; init; }

    [JsonPropertyName("summaryReport")]
    public SummaryReportCr? SummaryReportCr { get; init; }

    [JsonPropertyName("updateTimestamp")]
    public DateTime UpdateTimestamp { get; init; } = DateTime.MinValue;
}
