using System.Text.Json.Serialization;

namespace TrivyOperator.Dashboard.Infrastructure.Trivy.ReportSchemas.Sboms;

public class Tools
{
    [JsonPropertyName("components")]
    public ToolsComponent[] Components { get; init; } = [];
}
