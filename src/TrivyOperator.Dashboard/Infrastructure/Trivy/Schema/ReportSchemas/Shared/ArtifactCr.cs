using System.Text.Json.Serialization;

namespace TrivyOperator.Dashboard.Infrastructure.Trivy.Schema.ReportSchemas.Shared;

public class ArtifactCr
{
    [JsonPropertyName("digest")]
    public string? Digest { get; init; }
    
    [JsonPropertyName("mimeType")]
    public string? MimeType { get; init; }

    [JsonPropertyName("repository")]
    public string? Repository { get; init; }

    [JsonPropertyName("tag")]
    public string? Tag { get; init; }
}
