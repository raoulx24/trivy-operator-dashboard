using System.Text.Json.Serialization;

namespace TrivyOperator.Dashboard.Domain.Trivy.SbomReport;

public class Tools
{
    [JsonPropertyName("components")]
    public ToolsComponent[] Components { get; init; } = [];
}
