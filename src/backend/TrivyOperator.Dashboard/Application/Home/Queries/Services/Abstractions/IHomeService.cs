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

    // one id (exact match) of the new ones with everything: its new packages, all the images and namespaces; the limit does
    // not apply
    Task<HomeNewVulnerabilityDetailDto> GetNewVulnerabilityDetailed(
        string vulnerabilityId,
        HomeFilter filter,
        CancellationToken ctx = default);

    Task<HomeNewFixesDto> GetNewFixes(HomeFilter filter, CancellationToken ctx = default);

    // one id (exact match) with new fixes, with everything: its packages whose fix is new, all the images and namespaces;
    // the limit does not apply
    Task<HomeNewFixDetailDto> GetNewFixDetailed(
        string vulnerabilityId,
        HomeFilter filter,
        CancellationToken ctx = default);

    // the severities are not used: being exploited matters whatever the severity
    Task<HomeKnownExploitedDto> GetKnownExploited(HomeFilter filter, CancellationToken ctx = default);

    // one id (exact match) of the known exploited ones with everything: KEV / EUVD data, packages, all the images and
    // namespaces; the limit does not apply
    Task<HomeKnownExploitedDetailDto> GetKnownExploitedDetailed(
        string vulnerabilityId,
        HomeFilter filter,
        CancellationToken ctx = default);

    // the same data as the new vulnerabilities, flat: one row per (vulnerability id, package, image); not limited
    Task<HomeVulnerabilityRowsDto> GetNewVulnerabilitiesDenormalized(HomeFilter filter, CancellationToken ctx = default);

    // the same data as the new fixes, flat; not limited
    Task<HomeVulnerabilityRowsDto> GetNewFixesDenormalized(HomeFilter filter, CancellationToken ctx = default);

    // the same data as the known exploited ones, flat; not limited, the severities are not used
    Task<HomeVulnerabilityRowsDto> GetKnownExploitedDenormalized(HomeFilter filter, CancellationToken ctx = default);
    // the queries about the current state do not use the window (Since)
    Task<HomeBaseOsDto> GetBaseOs(HomeFilter filter, CancellationToken ctx = default);

    // not filtered and not limited: every running image per namespace, with its OS
    Task<HomeBaseOsImagesDto> GetBaseOsImages(CancellationToken ctx = default);

    Task<HomeFixWaitingDto> GetFixWaiting(HomeFilter filter, CancellationToken ctx = default);

    // one id (exact match) with fixes waiting, with everything: all its packages with a fix, all the images and
    // namespaces; the limit does not apply
    Task<HomeFixWaitingDetailDto> GetFixWaitingDetailed(
        string vulnerabilityId,
        HomeFilter filter,
        CancellationToken ctx = default);

    Task<HomeFailingChecksDto> GetFailingChecks(HomeFilter filter, CancellationToken ctx = default);

    // one check (family and id, exact match) with everything: description, remediation, one row per failing resource;
    // the limit does not apply
    Task<HomeFailingCheckDetailDto> GetFailingCheckDetailed(
        string family,
        string checkId,
        HomeFilter filter,
        CancellationToken ctx = default);

    Task<HomeOldestImagesDto> GetOldestImages(HomeFilter filter, CancellationToken ctx = default);

    // running images with exposed secrets (the severities are not used: an exposed secret matters whatever the severity)
    Task<HomeExposedSecretsDto> GetExposedSecrets(HomeFilter filter, CancellationToken ctx = default);

    Task<HomePackageHotspotsDto> GetPackageHotspots(HomeFilter filter, CancellationToken ctx = default);

    // minEpss: only the ids whose EPSS percentile is at least this (0..1); none = all
    Task<HomeReachEpssDto> GetReachEpss(HomeFilter filter, double? minEpss = null, CancellationToken ctx = default);

    // the cluster compliance reports with their pass percent (not filtered: the reports are about the cluster)
    Task<HomeComplianceDto> GetCompliance(CancellationToken ctx = default);

    // the worst failing controls of one report (exact uid), capped by the limit of the filter
    Task<HomeComplianceWorstControlsDto> GetComplianceWorstControls(
        string reportUid,
        HomeFilter filter,
        CancellationToken ctx = default);
}
