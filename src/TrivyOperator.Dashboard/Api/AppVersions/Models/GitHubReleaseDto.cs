using TrivyOperator.Dashboard.Infrastructure.Clients.GitHub.Models;

namespace TrivyOperator.Dashboard.Api.AppVersions.Models;

public class GitHubReleaseDto
{
    public string TagName { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public string HtmlUrl { get; set; } = string.Empty;
    public DateTime PublishedAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public bool IsLatest { get; set; }
}

public static class GitHubReleaseExtensions
{
    public static GitHubReleaseDto ToGitHubReleaseDto(this GitHubRelease release) => new()
    {
        TagName = release.TagName ?? string.Empty,
        Name = release.Name ?? string.Empty,
        Body = release.Body ?? string.Empty,
        HtmlUrl = release.HtmlUrl ?? string.Empty,
        PublishedAt = release.PublishedAt,
        CreatedAt = release.CreatedAt,
        IsLatest = release.IsLatest,
    };
}
