using System.Text.Json.Serialization;

namespace TrivyOperator.Dashboard.Domain.Trivy.RbacAssessmentReport;

public class Check
{
    [JsonPropertyName("category")]
    public string Category { get; init; } = string.Empty;

    [JsonPropertyName("checkID")]
    public string CheckId { get; init; } = string.Empty;

    [JsonPropertyName("description")]
    public string Description { get; init; } = string.Empty;

    [JsonPropertyName("messages")]
    public string[] Messages { get; init; } = [];

    [JsonPropertyName("remediation")]
    public string Remediation { get; init; } = string.Empty;

    [JsonPropertyName("severity")]
    public TrivySeverity Severity { get; init; }

    [JsonPropertyName("success")]
    public bool Success { get; init; } = false;

    [JsonPropertyName("title")]
    public string Title { get; init; } = string.Empty;
}
