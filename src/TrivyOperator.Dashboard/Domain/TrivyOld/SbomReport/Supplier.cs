using System.Text.Json.Serialization;

namespace TrivyOperator.Dashboard.Domain.TrivyOld.SbomReport;

public class Supplier
{
    [JsonPropertyName("email")]
    public string? Email { get; set; }

    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("phone")]
    public string? Phone { get; set; }
}
