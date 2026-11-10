using TrivyOperator.Dashboard.Application.Home.Queries.Models;
using TrivyOperator.Dashboard.Application.Home.Queries.Services.Abstractions;
using TrivyOperator.Dashboard.Application.Shared.Queries.Services;

namespace TrivyOperator.Dashboard.Application.Home.Queries.Services;

// multi-context and file repository: they get a different home page, these searches have nothing to return
public class HomeSearchNullService : QueryNullService, IHomeSearchService
{
    private const string Reason = "The home page data is available only for the default context";

    public Task<HomeWhereIsVulnerabilityDto> FindVulnerability(string query, HomeFilter filter, CancellationToken ctx = default) =>
        Task.FromResult(new HomeWhereIsVulnerabilityDto(false, Reason, query, null, 0, []));

    public Task<HomeWhereIsPackageDto> FindPackage(string query, HomeFilter filter, CancellationToken ctx = default) =>
        Task.FromResult(new HomeWhereIsPackageDto(false, Reason, query, null, string.Empty, 0, []));

    public Task<HomeWhereIsLicenseDto> FindLicense(string query, HomeFilter filter, CancellationToken ctx = default) =>
        Task.FromResult(new HomeWhereIsLicenseDto(false, Reason, query, null, 0, []));

    public Task<HomeWhereIsCheckDto> FindCheck(string query, HomeFilter filter, CancellationToken ctx = default) =>
        Task.FromResult(new HomeWhereIsCheckDto(false, Reason, query, null, 0, []));
}
