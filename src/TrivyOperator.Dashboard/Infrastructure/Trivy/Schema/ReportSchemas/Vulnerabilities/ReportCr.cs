using System.Text.Json.Serialization;
using TrivyOperator.Dashboard.Infrastructure.Trivy.Schema.ReportSchemas.Shared;

namespace TrivyOperator.Dashboard.Infrastructure.Trivy.Schema.ReportSchemas.Vulnerabilities;

public class ReportCr
{
    [JsonPropertyName("artifact")]
    public ArtifactCr? Artifact { get; set; }

    [JsonPropertyName("os")]
    public OsCr? Os { get; set; }

    [JsonPropertyName("registry")]
    public RegistryCr? Registry { get; set; }

    [JsonPropertyName("scanner")]
    public ScannerCr? Scanner { get; set; }

    [JsonPropertyName("summary")]
    public SummaryCr? Summary { get; set; }

    [JsonPropertyName("updateTimestamp")]
    public DateTime? UpdateTimestamp { get; set; }

    [JsonPropertyName("vulnerabilities")]
    public VulnerabilityCr[]? Vulnerabilities { get; set; }
}
