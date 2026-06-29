using System.Text.Json.Serialization;
using TrivyOperator.Dashboard.Infrastructure.Trivy.Schema.ReportSchemas.Shared;

namespace TrivyOperator.Dashboard.Infrastructure.Trivy.Schema.ReportSchemas.ClusterComplianceReports;

public class ControlCr
{
    [JsonPropertyName("checks")]
    public CheckCr[] Checks { get; init; } = [];

    [JsonPropertyName("commands")]
    public CheckCr[] Commands { get; init; } = [];

    [JsonPropertyName("description")]
    public string Description { get; init; } = string.Empty;

    [JsonPropertyName("id")]
    public string Id { get; init; } = string.Empty;

    [JsonPropertyName("name")]
    public string Name { get; init; } = string.Empty;

    [JsonPropertyName("severity")]
    public SeverityCr SeverityCr { get; init; }

    [JsonPropertyName("defaultStatus")]
    public string DefaultStatus { get; init; } = string.Empty;
}
