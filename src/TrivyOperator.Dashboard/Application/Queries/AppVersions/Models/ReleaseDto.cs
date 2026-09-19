using TrivyOperator.Dashboard.Domain.Releases.Entities;

namespace TrivyOperator.Dashboard.Application.Queries.AppVersions.Models;

public sealed record ReleaseDto(
    string TagName,
    string Name,
    string Body,
    string HtmlUrl,
    DateTime PublishedAt,
    DateTime CreatedAt,
    bool IsLatest);

public static class GitHubReleaseExtensions
{
    public static ReleaseDto ToReleaseDto(this Release release) =>
        new(
            TagName: release.Version.Value,
            Name: release.Name.Value,
            Body: release.Description.Value,
            HtmlUrl: release.Url.InitialValue,
            PublishedAt: release.PublishedAt.Value,
            CreatedAt: release.CreatedAt.Value,
            IsLatest: release.IsLatest);
}
