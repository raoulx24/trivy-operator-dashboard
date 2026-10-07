using TrivyOperator.Dashboard.Domain.Releases.Entities;
using TrivyOperator.Dashboard.Domain.Releases.ValueObjects;
using TrivyOperator.Dashboard.Domain.Shared.ValueObjects;
using TrivyOperator.Dashboard.Infrastructure.GitHub.Models;

namespace TrivyOperator.Dashboard.Infrastructure.GitHub.Mappers;

public static class GitHubReleaseMapper
{
    public static Release ToDomain(this GitHubRelease release)
    {
        return new Release(
            new ReleaseId(release.Id.ToString()),
            new ReleaseVersion(release.TagName),
            new ReleaseName(release.Name),
            new ReleaseDescription(release.Body),
            new ResourceUrl(release.HtmlUrl),
            new Timestamp(release.PublishedAt),
            new Timestamp(release.CreatedAt),
            release.IsLatest
        );
    }
}
