using System.Text.Json.Serialization;

namespace TrivyOperator.Dashboard.Infrastructure.Trivy.Schema.ReportSchemas.Sboms;

public class CdxTools
{
    [JsonPropertyName("components")]
    public CdxToolsComponent[] Components { get; init; } = [];
}
