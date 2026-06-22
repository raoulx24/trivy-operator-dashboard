using System.Text.Json.Serialization;

namespace TrivyOperator.Dashboard.Domain.Trivy.ExposedSecretReport;

public class Secret
{
    [JsonPropertyName("category")]
    public string Category { get; init; } = string.Empty;

    [JsonPropertyName("match")]
    public string Match { get; init; } = string.Empty;

    [JsonPropertyName("ruleID")]
    public string RuleId { get; init; } = string.Empty;

    [JsonPropertyName("severity")]
    public TrivySeverity Severity { get; init; }

    [JsonPropertyName("target")]
    public string Target { get; init; } = string.Empty;

    [JsonPropertyName("title")]
    public string Title { get; init; } = string.Empty;
}
