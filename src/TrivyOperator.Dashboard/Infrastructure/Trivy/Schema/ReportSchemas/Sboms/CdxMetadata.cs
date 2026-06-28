using System.Text.Json.Serialization;

namespace TrivyOperator.Dashboard.Infrastructure.Trivy.Schema.ReportSchemas.Sboms;

public class CdxMetadata
{
    [JsonPropertyName("component")]
    public CdxComponent CdxComponent { get; init; } = new();

    [JsonPropertyName("timestamp")]
    public DateTime? Timestamp { get; init; }

    [JsonPropertyName("tools")]
    public CdxTools CdxTools { get; init; } = new();
}
