using System.Text.Json.Serialization;

namespace TrivyOperator.Dashboard.Domain.VrHistory;

public class VrImageInfo
{
    [JsonPropertyName("repositoryImage")]
    public string RepositoryImage { get; }
    [JsonPropertyName("tag")]
    public string Tag { get; }
    [JsonPropertyName("registry")]
    public string Registry { get; }

    public VrImageInfo(string repositoryImage, string tag, string registry)
    {
        if (string.IsNullOrWhiteSpace(repositoryImage)) throw new ArgumentException("RepositoryImage cannot be null or empty.");
        if (string.IsNullOrWhiteSpace(tag)) throw new ArgumentException("Tag cannot be null or empty.");

        RepositoryImage = repositoryImage;
        Tag = tag;
        Registry = registry;
    }
}
