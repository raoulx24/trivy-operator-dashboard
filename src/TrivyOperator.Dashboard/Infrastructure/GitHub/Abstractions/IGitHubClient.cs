using TrivyOperator.Dashboard.Infrastructure.GitHub.Models;

namespace TrivyOperator.Dashboard.Infrastructure.GitHub.Abstractions;

public interface IGitHubClient
{
    Task<GitHubRelease?> GetLatestRelease(string baseRepoUrl, CancellationToken cancellationToken);
    Task<GitHubRelease[]?> GitHubReleases(string baseRepoUrl, CancellationToken cancellationToken);
}
