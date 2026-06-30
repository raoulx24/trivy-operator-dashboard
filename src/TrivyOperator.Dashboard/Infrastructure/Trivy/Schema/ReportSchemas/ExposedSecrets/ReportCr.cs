using System.Text.Json.Serialization;
using TrivyOperator.Dashboard.Infrastructure.Trivy.Schema.ReportSchemas.Shared;

namespace TrivyOperator.Dashboard.Infrastructure.Trivy.Schema.ReportSchemas.ExposedSecrets;

public class ReportCr
{
    [JsonPropertyName("artifact")]
    public ArtifactCr Artifact { get; init; } = new ArtifactCr();

    [JsonPropertyName("registry")]
    public RegistryCr Registry { get; init; } = new RegistryCr();

    [JsonPropertyName("scanner")]
    public ScannerCr Scanner { get; init; } = new ScannerCr();

    [JsonPropertyName("secrets")]
    public SecretCr[] Secrets { get; init; } = [];

    [JsonPropertyName("summary")]
    public SummaryCr Summary { get; init; } = new SummaryCr();

    [JsonPropertyName("updateTimestamp")]
    public DateTime UpdateTimestamp { get; init; } =  DateTime.UtcNow;
}
