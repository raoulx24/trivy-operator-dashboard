using System.Text.Json.Serialization;
using TrivyOperator.Dashboard.Infrastructure.Trivy.Schema.ReportSchemas.Shared;

namespace TrivyOperator.Dashboard.Infrastructure.Trivy.Schema.ReportSchemas.ExposedSecrets;

public class ReportCr
{
    [JsonPropertyName("artifact")]
    public ArtifactCr? Artifact { get; init; }

    [JsonPropertyName("registry")]
    public RegistryCr? Registry { get; init; }

    [JsonPropertyName("scanner")]
    public ScannerCr? Scanner { get; init; }

    [JsonPropertyName("secrets")]
    public SecretCr[]? Secrets { get; init; }

    [JsonPropertyName("summary")]
    public SummaryCr? Summary { get; init; }

    [JsonPropertyName("updateTimestamp")]
    public DateTime? UpdateTimestamp { get; init; }
}
