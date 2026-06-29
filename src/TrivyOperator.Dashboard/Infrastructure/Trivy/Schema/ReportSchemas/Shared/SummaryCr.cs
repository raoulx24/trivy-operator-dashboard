using System.Text.Json.Serialization;

namespace TrivyOperator.Dashboard.Infrastructure.Trivy.Schema.ReportSchemas.Shared;

public class SummaryCr
{
    [JsonPropertyName("criticalCount")]
    public long CriticalCount { get; init; }

    [JsonPropertyName("highCount")]
    public long HighCount { get; init; }

    [JsonPropertyName("mediumCount")]
    public long MediumCount { get; init; }
    
    [JsonPropertyName("lowCount")]
    public long LowCount { get; init; }
    
    [JsonPropertyName("unknownCount")]
    public long? UnknownCount { get; init; }
    
    [JsonPropertyName("noneCount")]
    public long? NoneCount { get; init; }
}
