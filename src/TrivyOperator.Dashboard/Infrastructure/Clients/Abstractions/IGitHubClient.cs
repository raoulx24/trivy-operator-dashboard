using TrivyOperator.Dashboard.Infrastructure.Clients.Models;

namespace TrivyOperator.Dashboard.Infrastructure.Clients.Abstractions;

public interface IGitHubClient
{
    Task<GitHubRelease?> GetLatestRelease(string baseRepoUrl, CancellationToken cancellationToken);
    Task<GitHubRelease[]?> GitHubReleases(string baseRepoUrl, CancellationToken cancellationToken);
}