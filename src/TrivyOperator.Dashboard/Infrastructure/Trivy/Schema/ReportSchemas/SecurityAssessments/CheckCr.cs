using System.Text.Json.Serialization;
using TrivyOperator.Dashboard.Infrastructure.Trivy.Schema.ReportSchemas.Shared;
using TrivyOperator.Dashboard.Infrastructure.Utils.JsonConverters;

namespace TrivyOperator.Dashboard.Infrastructure.Trivy.Schema.ReportSchemas.SecurityAssessments;

public class CheckCr
{
    [JsonPropertyName("category")]
    [JsonConverter(typeof(StringInternalsJsonConverter))]
    public string Category { get; init; } = string.Empty;

    [JsonPropertyName("checkID")]
    [JsonConverter(typeof(StringInternalsJsonConverter))]
    public string CheckId { get; init; } = string.Empty;

    [JsonPropertyName("description")]
    [JsonConverter(typeof(StringInternalsJsonConverter))]
    public string Description { get; init; } = string.Empty;

    [JsonPropertyName("messages")]
    public string[] Messages { get; init; } = [];

    [JsonPropertyName("remediation")]
    [JsonConverter(typeof(StringInternalsJsonConverter))]
    public string Remediation { get; init; } = string.Empty;

    [JsonPropertyName("severity")]
    public SeverityCr SeverityCr { get; init; }

    [JsonPropertyName("success")]
    public bool Success { get; set; }

    [JsonPropertyName("title")]
    [JsonConverter(typeof(StringInternalsJsonConverter))]
    public string Title { get; init; } = string.Empty;
}
