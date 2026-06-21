using System.Text.Json.Serialization;

namespace TrivyOperator.Dashboard.Infrastructure.Trivy.ReportSchemas.Shared;

public class Registry
{
    [JsonPropertyName("server")]
    public string Server { get; init; } = string.Empty;
}
