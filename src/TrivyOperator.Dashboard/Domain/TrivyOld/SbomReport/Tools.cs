using System.Text.Json.Serialization;

namespace TrivyOperator.Dashboard.Domain.TrivyOld.SbomReport;

public class Tools
{
    [JsonPropertyName("components")]
    public ToolsComponent[] Components { get; init; } = [];
}
