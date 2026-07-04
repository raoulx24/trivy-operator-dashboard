using System.Text.Json.Serialization;
using TrivyOperator.Dashboard.Infrastructure.Trivy.Schema.ReportSchemas.Shared;

namespace TrivyOperator.Dashboard.Infrastructure.Trivy.Schema.ReportSchemas.Vulnerabilities;

public class ReportCr
{
    [JsonPropertyName("artifact")]
    public ArtifactCr Artifact { get; set; } = new();

    [JsonPropertyName("os")]
    public OsCr Os { get; set; } = new();

    [JsonPropertyName("registry")]
    public RegistryCr? Registry { get; set; }

    [JsonPropertyName("scanner")]
    public ScannerCr Scanner { get; set; } = new();

    [JsonPropertyName("summary")]
    public SummaryCr Summary { get; set; } = new();

    [JsonPropertyName("updateTimestamp")]
    public DateTime UpdateTimestamp { get; set; } = DateTime.UtcNow;

    [JsonPropertyName("vulnerabilities")]
    public VulnerabilityCr[] Vulnerabilities { get; set; } = [];
}
