using System.Text.Json.Serialization;

namespace TrivyOperator.Dashboard.Domain.Trivy.ClusterComplianceReport;

public class Control
{
    [JsonPropertyName("checks")]
    public Check[] Checks { get; init; } = [];

    [JsonPropertyName("commands")]
    public Check[] Commands { get; init; } = [];

    [JsonPropertyName("description")]
    public string Description { get; init; } = string.Empty;

    [JsonPropertyName("id")]
    public string Id { get; init; } = string.Empty;

    [JsonPropertyName("name")]
    public string Name { get; init; } = string.Empty;

    [JsonPropertyName("severity")]
    public TrivySeverity Severity { get; init; }

    [JsonPropertyName("defaultStatus")]
    public string DefaultStatus { get; init; } = string.Empty;
}
