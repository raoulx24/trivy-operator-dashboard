using TrivyOperator.Dashboard.Application.Home.Queries.Models;
using TrivyOperator.Dashboard.Application.Shared.Queries.Services.Abstractions;

namespace TrivyOperator.Dashboard.Application.Home.Queries.Services.Abstractions;

// the "where is" searches of the home page, default context only; they look at what runs now, so of the filter only
// the namespaces and the limit are used (see HomeSearchDtos for the matching rules)
public interface IHomeSearchService : IQueryService
{
    Task<HomeWhereIsVulnerabilityDto> FindVulnerability(string query, HomeFilter filter, CancellationToken ctx = default);

    // one id (exact match) with everything: all images and namespaces, packages, KEV / EPSS
    Task<HomeWhereIsVulnerabilityDetailDto> FindVulnerabilityDetailed(
        string vulnerabilityId,
        HomeFilter filter,
        CancellationToken ctx = default);

    Task<HomeWhereIsPackageDto> FindPackage(string query, HomeFilter filter, CancellationToken ctx = default);

    // one version of a package (exact match on the package key and the version) with everything: all images and
    // namespaces, licenses, vulnerabilities
    Task<HomeWhereIsPackageDetailDto> FindPackageDetailed(
        string packageKey,
        string version,
        HomeFilter filter,
        CancellationToken ctx = default);

    Task<HomeWhereIsLicenseDto> FindLicense(string query, HomeFilter filter, CancellationToken ctx = default);

    // one license (exact match) with everything: all the package versions, images and namespaces
    Task<HomeWhereIsLicenseDetailDto> FindLicenseDetailed(string license, HomeFilter filter, CancellationToken ctx = default);

    Task<HomeWhereIsCheckDto> FindCheck(string query, HomeFilter filter, CancellationToken ctx = default);

    // one check (family and id, exact match) with everything: the denormalized rows (namespace, resource)
    Task<HomeWhereIsCheckDetailDto> FindCheckDetailed(
        string family,
        string checkId,
        HomeFilter filter,
        CancellationToken ctx = default);
}
