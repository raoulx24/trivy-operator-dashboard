using System.Text.Json.Serialization;
using TrivyOperator.Dashboard.Domain.Trivy.Report.Abstractions;

namespace TrivyOperator.Dashboard.Domain.Trivy.SbomReport;

public class Registry : IRegistry
{
    [JsonPropertyName("server")]
    public string Server { get; init; } = string.Empty;
}
