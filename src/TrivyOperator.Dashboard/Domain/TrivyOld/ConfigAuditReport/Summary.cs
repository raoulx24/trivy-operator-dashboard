using System.Text.Json.Serialization;

namespace TrivyOperator.Dashboard.Domain.Trivy.ConfigAuditReport;

public class Summary
{
    [JsonPropertyName("criticalCount")]
    public long CriticalCount { get; init; } = 0;

    [JsonPropertyName("highCount")]
    public long HighCount { get; init; } = 0;

    [JsonPropertyName("lowCount")]
    public long LowCount { get; init; } = 0;

    [JsonPropertyName("mediumCount")]
    public long MediumCount { get; init; } = 0;
}
