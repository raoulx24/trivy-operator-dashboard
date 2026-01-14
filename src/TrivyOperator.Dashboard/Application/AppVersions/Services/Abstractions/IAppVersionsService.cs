using TrivyOperator.Dashboard.Application.AppVersions.Models;

namespace TrivyOperator.Dashboard.Application.AppVersions.Services.Abstractions;

public interface IAppVersionsService
{
    Task<GitHubReleaseDto?> GetTrivyDashboardLatestRelease();
    Task<IList<GitHubReleaseDto>> GetTrivyDashboardReleases();
    AppVersion GetCurrentVersion();
}
