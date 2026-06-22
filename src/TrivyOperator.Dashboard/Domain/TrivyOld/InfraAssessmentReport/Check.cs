using System.Text.Json.Serialization;
using TrivyOperator.Dashboard.Domain.Utils.JsonConverters;

namespace TrivyOperator.Dashboard.Domain.Trivy.InfraAssessmentReport;

public class Check
{
    [JsonPropertyName("category")]
    [JsonConverter(typeof(StringInternalsJsonConverter))]
    public string Category { get; set; } = string.Empty;

    [JsonPropertyName("checkID")]
    [JsonConverter(typeof(StringInternalsJsonConverter))]
    public string CheckId { get; set; } = string.Empty;

    [JsonPropertyName("description")]
    [JsonConverter(typeof(StringInternalsJsonConverter))]
    public string Description { get; set; } = string.Empty;

    [JsonPropertyName("messages")]
    public string[] Messages { get; set; } = [];

    [JsonPropertyName("remediation")]
    [JsonConverter(typeof(StringInternalsJsonConverter))]
    public string Remediation { get; set; } = string.Empty;

    [JsonPropertyName("severity")]
    public TrivySeverity Severity { get; set; }

    [JsonPropertyName("success")]
    public bool Success { get; set; } = false;

    [JsonPropertyName("title")]
    [JsonConverter(typeof(StringInternalsJsonConverter))]
    public string Title { get; set; } = string.Empty;
}
