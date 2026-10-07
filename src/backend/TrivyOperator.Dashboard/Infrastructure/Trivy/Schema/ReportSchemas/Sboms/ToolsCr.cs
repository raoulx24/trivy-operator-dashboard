using System.Text.Json.Serialization;

namespace TrivyOperator.Dashboard.Infrastructure.Trivy.Schema.ReportSchemas.Sboms;

public class ToolsCr
{
    [JsonPropertyName("components")]
    public ToolsComponentCr[] Components { get; init; } = [];
}
