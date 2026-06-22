using System.Text.Json.Serialization;

namespace TrivyOperator.Dashboard.Domain.Trivy.ClusterSbomReport;

public class Tools
{
    [JsonPropertyName("components")]
    public ToolsComponent[] Components { get; init; } = [];
}
