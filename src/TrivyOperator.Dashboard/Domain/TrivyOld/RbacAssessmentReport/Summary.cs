using System.Text.Json.Serialization;

namespace TrivyOperator.Dashboard.Domain.TrivyOld.RbacAssessmentReport;

public class Summary
{
    [JsonPropertyName("criticalCount")]
    public int CriticalCount { get; init; } = 0;

    [JsonPropertyName("highCount")]
    public int HighCount { get; init; } = 0;

    [JsonPropertyName("lowCount")]
    public int LowCount { get; init; } = 0;

    [JsonPropertyName("mediumCount")]
    public int MediumCount { get; init; } = 0;
}
