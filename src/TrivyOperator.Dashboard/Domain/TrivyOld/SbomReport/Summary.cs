using System.Text.Json.Serialization;

namespace TrivyOperator.Dashboard.Domain.TrivyOld.SbomReport;

public class Summary
{
    [JsonPropertyName("componentsCount")]
    public int ComponentsCount { get; init; } = 0;

    [JsonPropertyName("dependenciesCount")]
    public int DependenciesCount { get; init; } = 0;
}
