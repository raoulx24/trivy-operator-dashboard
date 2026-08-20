using TrivyOperator.Dashboard.Application.Queries.AppVersions.Models;

namespace TrivyOperator.Dashboard.Application.Queries.AppVersions.Services.Abstractions;

public interface IAppVersionsService
{
    Task<GitHubReleaseDto?> GetTrivyDashboardLatestRelease();
    Task<IList<GitHubReleaseDto>> GetTrivyDashboardReleases();
    AppVersion GetCurrentVersion();
}
