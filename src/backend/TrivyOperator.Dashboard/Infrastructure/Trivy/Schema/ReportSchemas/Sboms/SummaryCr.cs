using System.Text.Json.Serialization;

namespace TrivyOperator.Dashboard.Infrastructure.Trivy.Schema.ReportSchemas.Sboms;

public class SummaryCr
{
    [JsonPropertyName("componentsCount")]
    public int ComponentsCount { get; init; } = 0;

    [JsonPropertyName("dependenciesCount")]
    public int DependenciesCount { get; init; } = 0;
}
