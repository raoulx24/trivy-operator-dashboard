using System.Text.Json.Serialization;
using TrivyOperator.Dashboard.Infrastructure.Trivy.Schema.ReportSchemas.SecurityAssessments;
using TrivyOperator.Dashboard.Infrastructure.Utils.JsonConverters;

namespace TrivyOperator.Dashboard.Infrastructure.Trivy.Schema.ReportSchemas.Shared;

public class SecurityAssessmentCheckCr
{
    [JsonPropertyName("category")]
    [JsonConverter(typeof(StringInternalsJsonConverter))]
    public string? Category { get; init; }

    [JsonPropertyName("checkID")]
    [JsonConverter(typeof(StringInternalsJsonConverter))]
    public string CheckId { get; init; } = string.Empty;

    [JsonPropertyName("description")]
    [JsonConverter(typeof(StringInternalsJsonConverter))]
    public string? Description { get; init; }

    [JsonPropertyName("messages")]
    public string[]? Messages { get; init; }

    [JsonPropertyName("remediation")]
    [JsonConverter(typeof(StringInternalsJsonConverter))]
    public string? Remediation { get; init; }
    
    [JsonPropertyName("scope")]
    public CheckScopeCr? Scope { get; init; }

    [JsonPropertyName("severity")]
    public SeverityCr SeverityCr { get; init; }

    [JsonPropertyName("success")]
    public bool Success { get; set; }

    [JsonPropertyName("title")]
    [JsonConverter(typeof(StringInternalsJsonConverter))]
    public string? Title { get; init; }
}
