using TrivyOperator.Dashboard.Application.Home.Queries.Models;
using TrivyOperator.Dashboard.Application.Shared.Queries.Services.Abstractions;

namespace TrivyOperator.Dashboard.Application.Home.Queries.Services.Abstractions;

// the home page queries, default context only (see HomeFilter for the filters)
public interface IHomeService : IQueryService
{
    // not filtered: it is about the data as a whole
    Task<HomeDataHealthDto> GetDataHealth(CancellationToken ctx = default);

    // the namespaces that have something for the home page (running images with vulnerability reports, or security
    // assessment reports), sorted; the choices of the namespaces filter
    Task<IReadOnlyList<string>> GetNamespaces(CancellationToken ctx = default);

    Task<HomeNewVulnerabilitiesDto> GetNewVulnerabilities(HomeFilter filter, CancellationToken ctx = default);

    Task<HomeNewFixesDto> GetNewFixes(HomeFilter filter, CancellationToken ctx = default);

    // the severities are not used: being exploited matters whatever the severity
    Task<HomeKnownExploitedDto> GetKnownExploited(HomeFilter filter, CancellationToken ctx = default);

    // the queries about the current state do not use the window (Since)
    Task<HomeBaseOsDto> GetBaseOs(HomeFilter filter, CancellationToken ctx = default);

    Task<HomeFixWaitingDto> GetFixWaiting(HomeFilter filter, CancellationToken ctx = default);

    Task<HomeFailingChecksDto> GetFailingChecks(HomeFilter filter, CancellationToken ctx = default);

    Task<HomeOldestImagesDto> GetOldestImages(HomeFilter filter, CancellationToken ctx = default);

    Task<HomePackageHotspotsDto> GetPackageHotspots(HomeFilter filter, CancellationToken ctx = default);

    Task<HomeReachEpssDto> GetReachEpss(HomeFilter filter, CancellationToken ctx = default);
}
