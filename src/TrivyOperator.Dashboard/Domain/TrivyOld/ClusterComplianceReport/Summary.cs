using System.Text.Json.Serialization;

namespace TrivyOperator.Dashboard.Domain.TrivyOld.ClusterComplianceReport;

public class Summary
{
    [JsonPropertyName("failCount")]
    public int FailCount { get; init; } = 0;

    [JsonPropertyName("passCount")]
    public int PassCount { get; init; } = 0;
}
