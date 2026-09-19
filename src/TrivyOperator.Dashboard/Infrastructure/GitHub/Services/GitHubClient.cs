using System.Text.Json;
using TrivyOperator.Dashboard.Application.Releases.Abstractions;
using TrivyOperator.Dashboard.Domain.Releases.Entities;
using TrivyOperator.Dashboard.Infrastructure.GitHub.Mappers;
using TrivyOperator.Dashboard.Infrastructure.GitHub.Models;

namespace TrivyOperator.Dashboard.Infrastructure.GitHub.Services;

public class GitHubClient(HttpClient httpClient, ILogger<GitHubClient> logger) : IReleaseProvider
{
    public async Task<Release?> GetLatestRelease(string baseRepoUrl, CancellationToken ctx = default)
    {
        try
        {
            HttpResponseMessage response = await httpClient.GetAsync(
                $"{baseRepoUrl.TrimEnd('/')}/releases/latest",
                ctx
            );
            response.EnsureSuccessStatusCode();
            string content = await response.Content.ReadAsStringAsync(ctx);
            GitHubRelease? gitHubRelease = JsonSerializer.Deserialize<GitHubRelease>(content);

            return gitHubRelease?.ToDomain();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error fetching latest release from GitHub.");
            return null;
        }
    }

    public async Task<Release[]?> GitHubReleases(string baseRepoUrl, CancellationToken ctx = default)
    {
        try
        {
            HttpResponseMessage response = await httpClient.GetAsync(
                $"{baseRepoUrl.TrimEnd('/')}/releases",
                ctx
            );
            response.EnsureSuccessStatusCode();
            string content = await response.Content.ReadAsStringAsync(ctx);
            GitHubRelease[]? gitHubReleases = JsonSerializer.Deserialize<GitHubRelease[]>(content);
            
            return gitHubReleases is null 
                ? null
                : [.. gitHubReleases.Select(x => x.ToDomain()),];
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error fetching releases from GitHub.");
            return null;
        }
    }
}
