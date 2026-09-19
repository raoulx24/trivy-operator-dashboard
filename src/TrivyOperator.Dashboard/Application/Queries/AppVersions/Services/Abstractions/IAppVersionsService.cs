using TrivyOperator.Dashboard.Application.Queries.AppVersions.Models;

namespace TrivyOperator.Dashboard.Application.Queries.AppVersions.Services.Abstractions;

public interface IAppVersionsService
{
    Task<ReleaseDto?> GetTrivyDashboardLatestRelease();
    Task<IList<ReleaseDto>> GetTrivyDashboardReleases();
    AppVersion GetCurrentVersion();
}
