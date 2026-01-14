using System.Text.Json.Serialization;

namespace TrivyOperator.Dashboard.Infrastructure.Clients.Models;

public class GitHubRelease
{
    [JsonPropertyName("id")]
    public long Id { get; init; }

    [JsonPropertyName("tag_name")]
    public string? TagName { get; init; }

    [JsonPropertyName("name")]
    public string? Name { get; init; }

    [JsonPropertyName("body")]
    public string? Body { get; init; }

    [JsonPropertyName("url")]
    public string? HtmlUrl { get; init; }

    [JsonPropertyName("published_at")]
    public DateTime PublishedAt { get; init; }

    [JsonPropertyName("created_at")]
    public DateTime CreatedAt { get; init; }

    [JsonIgnore]
    public bool IsLatest { get; set; }
}
