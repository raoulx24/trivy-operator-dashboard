using System.Text.Json.Serialization;

namespace TrivyOperator.Dashboard.Infrastructure.Trivy.Schema.ReportSchemas.Shared;

public class Registry
{
    [JsonPropertyName("server")]
    public string Server { get; init; } = string.Empty;
}
