using System.Text.Json;
using TrivyOperator.Dashboard.Infrastructure.Abstractions;
using TrivyOperator.Dashboard.Infrastructure.Clients.Models;

namespace TrivyOperator.Dashboard.Infrastructure.Clients;

public class GitHubClient(HttpClient httpClient, ILogger<GitHubClient> logger) : IGitHubClient
{
    public async Task<GitHubRelease?> GetLatestRelease(string baseRepoUrl, CancellationToken cancellationToken)
    {
        try
        {
            HttpResponseMessage response = await httpClient.GetAsync($"{baseRepoUrl.TrimEnd('/')}/releases/latest", cancellationToken);
            response.EnsureSuccessStatusCode();
            string content = await response.Content.ReadAsStringAsync(cancellationToken);
            return JsonSerializer.Deserialize<GitHubRelease>(content);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error fetching latest release from GitHub.");
            return null;
        }
    }
    
    public async Task<GitHubRelease[]?> GitHubReleases(string baseRepoUrl, CancellationToken cancellationToken)
    {
        try
        {
            HttpResponseMessage response = await httpClient.GetAsync($"{baseRepoUrl.TrimEnd('/')}/releases", cancellationToken);
            response.EnsureSuccessStatusCode();
            string content = await response.Content.ReadAsStringAsync(cancellationToken);
            return JsonSerializer.Deserialize<GitHubRelease[]>(content);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error fetching releases from GitHub.");
            return null;
        }
    }
}
