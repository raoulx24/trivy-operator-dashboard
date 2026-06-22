using System.Text.Json.Serialization;

namespace TrivyOperator.Dashboard.Domain.Trivy.ClusterSbomReport;

public class Registry
{
    [JsonPropertyName("server")]
    public string Server { get; init; } = string.Empty;
}
