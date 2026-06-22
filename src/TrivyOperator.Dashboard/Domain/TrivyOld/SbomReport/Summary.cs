using System.Text.Json.Serialization;

namespace TrivyOperator.Dashboard.Domain.Trivy.SbomReport;

public class Summary
{
    [JsonPropertyName("componentsCount")]
    public long ComponentsCount { get; init; } = 0;

    [JsonPropertyName("dependenciesCount")]
    public long DependenciesCount { get; init; } = 0;
}
