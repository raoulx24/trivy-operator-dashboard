using System.Text.Json.Serialization;
using TrivyOperator.Dashboard.Infrastructure.Trivy.Schema.ReportSchemas.Shared;

namespace TrivyOperator.Dashboard.Infrastructure.Trivy.Schema.ReportSchemas.Sboms;

public class ReportCr
{
    [JsonPropertyName("artifact")]
    public ArtifactCr Artifact { get; init; } = new();

    [JsonPropertyName("components")]
    public ComponentsCr Components { get; init; } = new();

    [JsonPropertyName("registry")]
    public RegistryCr Registry { get; init; } = new();

    [JsonPropertyName("scanner")]
    public ScannerCr Scanner { get; init; } = new();

    [JsonPropertyName("summary")]
    public SummaryCr Summary { get; init; } = new();

    [JsonPropertyName("updateTimestamp")]
    public DateTime? UpdateTimestamp { get; init; }
}
