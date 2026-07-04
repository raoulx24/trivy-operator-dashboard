using System.Text.Json.Serialization;

namespace TrivyOperator.Dashboard.Infrastructure.Trivy.Schema.ReportSchemas.Shared;

public class RegistryCr
{
    [JsonPropertyName("server")]
    public string? Server { get; init; }
}
