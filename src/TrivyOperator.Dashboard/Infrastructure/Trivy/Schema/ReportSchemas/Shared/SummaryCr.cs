using System.Text.Json.Serialization;

namespace TrivyOperator.Dashboard.Infrastructure.Trivy.Schema.ReportSchemas.Shared;

public class SummaryCr
{
    [JsonPropertyName("criticalCount")]
    public int CriticalCount { get; init; }

    [JsonPropertyName("highCount")]
    public int HighCount { get; init; }

    [JsonPropertyName("mediumCount")]
    public int MediumCount { get; init; }
    
    [JsonPropertyName("lowCount")]
    public int LowCount { get; init; }
    
    [JsonPropertyName("unknownCount")]
    public int? UnknownCount { get; init; }
    
    [JsonPropertyName("noneCount")]
    public int? NoneCount { get; init; }
}
