using System.Text.Json.Serialization;

namespace TrivyOperator.Dashboard.Domain.TrivyOld.SbomReport;

public class LicenseContainer
{
    [JsonPropertyName("license")]
    public License? License { get; set; }
}

public class License
{
    [JsonPropertyName("id")]
    public string? Id { get; init; }

    [JsonPropertyName("name")]
    public string? Name { get; init; }

    [JsonPropertyName("url")]
    public string? Url { get; init; }
}
