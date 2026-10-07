using TrivyOperator.Dashboard.Application.AppVersions.Queries.Models;

namespace TrivyOperator.Dashboard.Application.AppVersions.Queries.Services.Abstractions;

public interface IAppVersionsService
{
    Task<ReleaseDto?> GetTrivyDashboardLatestRelease();
    Task<IList<ReleaseDto>> GetTrivyDashboardReleases();
    AppVersion GetCurrentVersion();
}
