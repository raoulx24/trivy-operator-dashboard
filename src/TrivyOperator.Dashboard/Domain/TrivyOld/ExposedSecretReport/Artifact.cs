using System.Text.Json.Serialization;
using TrivyOperator.Dashboard.Domain.TrivyOld.Report.Abstractions;

namespace TrivyOperator.Dashboard.Domain.TrivyOld.ExposedSecretReport;

public class Artifact : IArtifact
{
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("digest")]
    public string Digest { get; init; } = string.Empty;

    [JsonPropertyName("repository")]
    public string Repository { get; init; } = string.Empty;

    [JsonPropertyName("tag")]
    public string Tag { get; init; } = string.Empty;
}
