using System.Text.Json.Serialization;
using TrivyOperator.Dashboard.Domain.TrivyOld.Report.Abstractions;

namespace TrivyOperator.Dashboard.Domain.TrivyOld.ExposedSecretReport;

public class Registry : IRegistry
{
    [JsonPropertyName("server")]
    public string Server { get; init; } = string.Empty;
}
