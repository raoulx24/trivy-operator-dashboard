
using TrivyOperator.Dashboard.Domain.Releases.Entities;

namespace TrivyOperator.Dashboard.Application.Releases.Abstractions;

public interface IReleaseProvider
{
    Task<Release?> GetLatestRelease(string baseUrl, CancellationToken ctx = default);
    Task<Release[]?> GitHubReleases(string baseUrl, CancellationToken ctx = default);
}
