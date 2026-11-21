using TrivyOperator.Dashboard.Application.Home.Queries.Models;
using TrivyOperator.Dashboard.Application.Home.Queries.Services.Abstractions;
using TrivyOperator.Dashboard.Application.Shared.Queries.Services;

namespace TrivyOperator.Dashboard.Application.Home.Queries.Services;

// multi-context and file repository: they get a different home page, these queries have nothing to return
public class HomeNullService : QueryNullService, IHomeService
{
    private const string Reason = "The home page data is available only for the default context";

    private static readonly HomeTotalsDto EmptyTotals = new(0, 0, 0, 0, 0);

    public Task<HomeDataHealthDto> GetDataHealth(CancellationToken ctx = default) =>
        Task.FromResult(new HomeDataHealthDto(0, 0, [], [], [], false, null, null, DateTime.UtcNow));

    public Task<IReadOnlyList<string>> GetNamespaces(CancellationToken ctx = default) =>
        Task.FromResult<IReadOnlyList<string>>([]);

    public Task<HomeNewVulnerabilitiesDto> GetNewVulnerabilities(HomeFilter filter, CancellationToken ctx = default) =>
        Task.FromResult(new HomeNewVulnerabilitiesDto(false, Reason, filter.Since, null, EmptyTotals, []));

    public Task<HomeNewVulnerabilityDetailDto> GetNewVulnerabilityDetailed(
        string vulnerabilityId,
        HomeFilter filter,
        CancellationToken ctx = default) =>
        Task.FromResult(new HomeNewVulnerabilityDetailDto(false, Reason, vulnerabilityId, filter.Since, null, null));

    public Task<HomeNewFixesDto> GetNewFixes(HomeFilter filter, CancellationToken ctx = default) =>
        Task.FromResult(new HomeNewFixesDto(false, Reason, filter.Since, null, EmptyTotals, []));

    public Task<HomeNewFixDetailDto> GetNewFixDetailed(
        string vulnerabilityId,
        HomeFilter filter,
        CancellationToken ctx = default) =>
        Task.FromResult(new HomeNewFixDetailDto(false, Reason, vulnerabilityId, filter.Since, null, null));

    public Task<HomeKnownExploitedDto> GetKnownExploited(HomeFilter filter, CancellationToken ctx = default) =>
        Task.FromResult(new HomeKnownExploitedDto(false, Reason, filter.Since, 0, EmptyTotals, []));

    public Task<HomeKnownExploitedDetailDto> GetKnownExploitedDetailed(
        string vulnerabilityId,
        HomeFilter filter,
        CancellationToken ctx = default) =>
        Task.FromResult(new HomeKnownExploitedDetailDto(false, Reason, vulnerabilityId, filter.Since, null, null));

    public Task<HomeVulnerabilityRowsDto> GetNewVulnerabilitiesDenormalized(
        HomeFilter filter,
        CancellationToken ctx = default) =>
        Task.FromResult(new HomeVulnerabilityRowsDto(false, Reason, filter.Since, []));

    public Task<HomeVulnerabilityRowsDto> GetNewFixesDenormalized(HomeFilter filter, CancellationToken ctx = default) =>
        Task.FromResult(new HomeVulnerabilityRowsDto(false, Reason, filter.Since, []));

    public Task<HomeVulnerabilityRowsDto> GetKnownExploitedDenormalized(
        HomeFilter filter,
        CancellationToken ctx = default) =>
        Task.FromResult(new HomeVulnerabilityRowsDto(false, Reason, filter.Since, []));
    public Task<HomeBaseOsDto> GetBaseOs(HomeFilter filter, CancellationToken ctx = default) =>
        Task.FromResult(new HomeBaseOsDto(false, Reason, []));

    public Task<HomeBaseOsImagesDto> GetBaseOsImages(CancellationToken ctx = default) =>
        Task.FromResult(new HomeBaseOsImagesDto(false, Reason, []));

    public Task<HomeFixWaitingDto> GetFixWaiting(HomeFilter filter, CancellationToken ctx = default) =>
        Task.FromResult(new HomeFixWaitingDto(false, Reason, null, EmptyTotals, []));

    public Task<HomeFixWaitingDetailDto> GetFixWaitingDetailed(
        string vulnerabilityId,
        HomeFilter filter,
        CancellationToken ctx = default) =>
        Task.FromResult(new HomeFixWaitingDetailDto(false, Reason, vulnerabilityId, null, null));

    public Task<HomeFailingChecksDto> GetFailingChecks(HomeFilter filter, CancellationToken ctx = default) =>
        Task.FromResult(new HomeFailingChecksDto(false, Reason, 0, []));

    public Task<HomeFailingCheckDetailDto> GetFailingCheckDetailed(
        string family,
        string checkId,
        HomeFilter filter,
        CancellationToken ctx = default) =>
        Task.FromResult(new HomeFailingCheckDetailDto(false, Reason, family, checkId, null, null));

    public Task<HomeOldestImagesDto> GetOldestImages(HomeFilter filter, CancellationToken ctx = default) =>
        Task.FromResult(new HomeOldestImagesDto(false, Reason, null, []));

    public Task<HomeExposedSecretsDto> GetExposedSecrets(HomeFilter filter, CancellationToken ctx = default) =>
        Task.FromResult(new HomeExposedSecretsDto(false, Reason, 0, null, 0, 0, []));

    public Task<HomePackageHotspotsDto> GetPackageHotspots(HomeFilter filter, CancellationToken ctx = default) =>
        Task.FromResult(new HomePackageHotspotsDto(false, Reason, []));

    public Task<HomeReachEpssDto> GetReachEpss(HomeFilter filter, CancellationToken ctx = default) =>
        Task.FromResult(new HomeReachEpssDto(false, Reason, []));
}
