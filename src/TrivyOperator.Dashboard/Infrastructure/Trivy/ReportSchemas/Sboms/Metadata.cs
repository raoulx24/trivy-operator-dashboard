using System.Text.Json.Serialization;

namespace TrivyOperator.Dashboard.Infrastructure.Trivy.ReportSchemas.Sboms;

public class Metadata
{
    [JsonPropertyName("component")]
    public Component Component { get; init; } = new();

    [JsonPropertyName("timestamp")]
    public DateTime? Timestamp { get; init; }

    [JsonPropertyName("tools")]
    public Tools Tools { get; init; } = new();
}
