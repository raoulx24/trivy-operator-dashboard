using System.Reflection;
using TrivyOperator.Dashboard.Application.Queries.AppVersions.Models;
using TrivyOperator.Dashboard.Application.Queries.AppVersions.Services.Abstractions;
using TrivyOperator.Dashboard.Application.Shared.Abstractions;
using TrivyOperator.Dashboard.Domain.Releases.Entities;

namespace TrivyOperator.Dashboard.Application.Queries.AppVersions.Services;

public class AppVersionsService(ICache<string, Release> cache) : IAppVersionsService
{
    public Task<ReleaseDto?> GetTrivyDashboardLatestRelease()
    {
        Release? release = cache.Select(x => x.Value).FirstOrDefault(x => x.IsLatest);
        return Task.FromResult(release?.ToReleaseDto());
    }

    public Task<IList<ReleaseDto>> GetTrivyDashboardReleases()
    {
        List<ReleaseDto> releases = [.. cache.Select(x => x.Value.ToReleaseDto()),];
        return Task.FromResult<IList<ReleaseDto>>(releases);
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
