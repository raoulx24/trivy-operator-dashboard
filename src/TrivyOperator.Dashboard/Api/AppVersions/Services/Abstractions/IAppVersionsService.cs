using TrivyOperator.Dashboard.Api.AppVersions.Models;

namespace TrivyOperator.Dashboard.Api.AppVersions.Services.Abstractions;

public interface IAppVersionsService
{
    Task<GitHubReleaseDto?> GetTrivyDashboardLatestRelease();
    Task<IList<GitHubReleaseDto>> GetTrivyDashboardReleases();
    AppVersion GetCurrentVersion();
}
