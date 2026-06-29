using System.Text.Json.Serialization;

namespace TrivyOperator.Dashboard.Domain.TrivyOld.ClusterSbomReport;

public class Registry
{
    [JsonPropertyName("server")]
    public string Server { get; init; } = string.Empty;
}
