using System.Text.Json.Serialization;

namespace TrivyOperator.Dashboard.Infrastructure.Trivy.Schema.ReportSchemas.Sboms;

public class MetadataCr
{
    [JsonPropertyName("component")]
    public ComponentCr? ComponentCr { get; init; }

    [JsonPropertyName("timestamp")]
    public DateTime? Timestamp { get; init; }

    [JsonPropertyName("tools")]
    public ToolsCr? ToolsCr { get; init; }
}
