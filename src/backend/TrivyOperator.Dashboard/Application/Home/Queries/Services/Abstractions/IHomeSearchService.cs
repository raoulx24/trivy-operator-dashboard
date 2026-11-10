using TrivyOperator.Dashboard.Application.Home.Queries.Models;
using TrivyOperator.Dashboard.Application.Shared.Queries.Services.Abstractions;

namespace TrivyOperator.Dashboard.Application.Home.Queries.Services.Abstractions;

// the "where is" searches of the home page, default context only; they look at what runs now, so of the filter only
// the namespaces and the limit are used (see HomeSearchDtos for the matching rules)
public interface IHomeSearchService : IQueryService
{
    Task<HomeWhereIsVulnerabilityDto> FindVulnerability(string query, HomeFilter filter, CancellationToken ctx = default);

    Task<HomeWhereIsPackageDto> FindPackage(string query, HomeFilter filter, CancellationToken ctx = default);

    Task<HomeWhereIsLicenseDto> FindLicense(string query, HomeFilter filter, CancellationToken ctx = default);

    Task<HomeWhereIsCheckDto> FindCheck(string query, HomeFilter filter, CancellationToken ctx = default);
}
