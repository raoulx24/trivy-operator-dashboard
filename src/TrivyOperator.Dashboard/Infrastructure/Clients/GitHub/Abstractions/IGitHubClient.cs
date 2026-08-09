using TrivyOperator.Dashboard.Infrastructure.Clients.GitHub.Models;

namespace TrivyOperator.Dashboard.Infrastructure.Clients.GitHub.Abstractions;

public interface IGitHubClient
{
    Task<GitHubRelease?> GetLatestRelease(string baseRepoUrl, CancellationToken cancellationToken);
    Task<GitHubRelease[]?> GitHubReleases(string baseRepoUrl, CancellationToken cancellationToken);
}
