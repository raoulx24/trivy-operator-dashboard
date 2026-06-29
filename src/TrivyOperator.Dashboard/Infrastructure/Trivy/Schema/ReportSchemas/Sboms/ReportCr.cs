using System.Text.Json.Serialization;
using TrivyOperator.Dashboard.Infrastructure.Trivy.Schema.ReportSchemas.Shared;

namespace TrivyOperator.Dashboard.Infrastructure.Trivy.Schema.ReportSchemas.Sboms;

public class ReportCr
{
    [JsonPropertyName("artifact")]
    public ArtifactCr ArtifactCr { get; init; } = new();

    [JsonPropertyName("components")]
    public ComponentsCr ComponentsCr { get; init; } = new();

    [JsonPropertyName("registry")]
    public RegistryCr RegistryCr { get; init; } = new();

    [JsonPropertyName("scanner")]
    public ScannerCr ScannerCr { get; init; } = new();

    [JsonPropertyName("summary")]
    public SummaryCr SummaryCr { get; init; } = new();

    [JsonPropertyName("updateTimestamp")]
    public DateTime? UpdateTimestamp { get; init; }
}
