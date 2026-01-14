using System.Reflection;
using TrivyOperator.Dashboard.Application.AppVersions.Models;
using TrivyOperator.Dashboard.Application.AppVersions.Services.Abstractions;
using TrivyOperator.Dashboard.Infrastructure.Caching.Abstractions;
using TrivyOperator.Dashboard.Infrastructure.Clients.Models;

namespace TrivyOperator.Dashboard.Application.AppVersions.Services;

public class AppVersionsService(IConcurrentCache<long, GitHubRelease> cache) : IAppVersionsService
{
    public Task<GitHubReleaseDto?> GetTrivyDashboardLatestRelease()
    {
        GitHubRelease? release = cache.Select(x => x.Value).FirstOrDefault(x => x.IsLatest);
        return Task.FromResult(release?.ToGitHubReleaseDto());
    }

    public Task<IList<GitHubReleaseDto>> GetTrivyDashboardReleases()
    {
        List<GitHubReleaseDto> releases = [.. cache.Select(x => x.Value.ToGitHubReleaseDto()),];
        return Task.FromResult<IList<GitHubReleaseDto>>(releases);
    }

    public AppVersion GetCurrentVersion()
    {
        Assembly assembly = Assembly.GetExecutingAssembly();

        return new AppVersion
        {
            FileVersion = assembly.GetCustomAttribute<AssemblyFileVersionAttribute>()?.Version ?? "0.0",
            InformationalVersion =
                assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion ?? "0.0",
        };
    }
}
