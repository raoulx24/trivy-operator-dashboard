using System.Text.Json.Serialization;

namespace TrivyOperator.Dashboard.Domain.TrivyOld.InfraAssessmentReport;

public class Report
{
    [JsonPropertyName("checks")]
    public Check[] Checks { get; init; } = [];

    [JsonPropertyName("scanner")]
    public Scanner Scanner { get; init; } = new();

    [JsonPropertyName("summary")]
    public Summary Summary { get; init; } = new();
}
