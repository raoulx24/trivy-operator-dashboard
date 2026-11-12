using Microsoft.AspNetCore.Mvc;
using TrivyOperator.Dashboard.Application.Home.Queries.Models;
using TrivyOperator.Dashboard.Application.Home.Queries.Services.Abstractions;

namespace TrivyOperator.Dashboard.Api.Home.Controllers;

// the home page, one endpoint per widget
// since: start of the "since last visit" window (UTC, default: 24 hours ago); used only by the endpoints about changes
// severities: severity ids (0 critical ... 4 unknown, default: critical and high), e.g. ?severities=0&severities=1
// namespaces: only what runs in these namespaces (cluster scoped ones are then out), e.g. ?namespaces=a&namespaces=b
// limit: maximum number of items of a list
[ApiController]
[Route("api/home")]
public class HomeController(
    IHomeService homeService,
    IHomeSearchService homeSearchService,
    IHomeTimelineService homeTimelineService) : ControllerBase
{
    private const int DefaultLimit = 20;
    private const int MaxLimit = 500;

    [HttpGet("data-health", Name = "GetHomeDataHealth")]
    [ProducesResponseType(typeof(HomeDataHealthDto), StatusCodes.Status200OK, "application/json")]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError, "application/problem+json")]
    public async Task<IActionResult> GetDataHealth(CancellationToken ctx) =>
        Ok(await homeService.GetDataHealth(ctx));

    [HttpGet("namespaces", Name = "GetHomeNamespaces")]
    [ProducesResponseType(typeof(IReadOnlyList<string>), StatusCodes.Status200OK, "application/json")]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError, "application/problem+json")]
    public async Task<IActionResult> GetNamespaces(CancellationToken ctx) =>
        Ok(await homeService.GetNamespaces(ctx));

    [HttpGet("new-vulnerabilities", Name = "GetHomeNewVulnerabilities")]
    [ProducesResponseType(typeof(HomeNewVulnerabilitiesDto), StatusCodes.Status200OK, "application/json")]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError, "application/problem+json")]
    public async Task<IActionResult> GetNewVulnerabilities(
        [FromQuery] DateTime? since,
        [FromQuery] int[]? severities,
        [FromQuery] string[]? namespaces,
        [FromQuery] int? limit,
        CancellationToken ctx) =>
        Ok(await homeService.GetNewVulnerabilities(ToFilter(since, severities, namespaces, limit), ctx));

    [HttpGet("new-fixes", Name = "GetHomeNewFixes")]
    [ProducesResponseType(typeof(HomeNewFixesDto), StatusCodes.Status200OK, "application/json")]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError, "application/problem+json")]
    public async Task<IActionResult> GetNewFixes(
        [FromQuery] DateTime? since,
        [FromQuery] int[]? severities,
        [FromQuery] string[]? namespaces,
        [FromQuery] int? limit,
        CancellationToken ctx) =>
        Ok(await homeService.GetNewFixes(ToFilter(since, severities, namespaces, limit), ctx));

    [HttpGet("known-exploited", Name = "GetHomeKnownExploited")]
    [ProducesResponseType(typeof(HomeKnownExploitedDto), StatusCodes.Status200OK, "application/json")]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError, "application/problem+json")]
    public async Task<IActionResult> GetKnownExploited(
        [FromQuery] DateTime? since,
        [FromQuery] string[]? namespaces,
        [FromQuery] int? limit,
        CancellationToken ctx) =>
        Ok(await homeService.GetKnownExploited(ToFilter(since, null, namespaces, limit), ctx));

    // days: the range ending today (UTC), at most 366; default: the history retention
    [HttpGet("history-drift", Name = "GetHomeHistoryDrift")]
    [ProducesResponseType(typeof(HomeHistoryDriftDto), StatusCodes.Status200OK, "application/json")]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError, "application/problem+json")]
    public async Task<IActionResult> GetHistoryDrift(
        [FromQuery] int? days,
        [FromQuery] int[]? severities,
        [FromQuery] string[]? namespaces,
        CancellationToken ctx) =>
        Ok(await homeTimelineService.GetHistoryDrift(days, ToFilter(null, severities, namespaces, null), ctx));

    // days: the range ending today (UTC), at most 366; default: the history retention
    [HttpGet("new-images", Name = "GetHomeNewImages")]
    [ProducesResponseType(typeof(HomeNewImagesDto), StatusCodes.Status200OK, "application/json")]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError, "application/problem+json")]
    public async Task<IActionResult> GetNewImages(
        [FromQuery] int? days,
        [FromQuery] int[]? severities,
        [FromQuery] string[]? namespaces,
        [FromQuery] int? limit,
        CancellationToken ctx) =>
        Ok(await homeTimelineService.GetNewImages(days, ToFilter(null, severities, namespaces, limit), ctx));

    [HttpGet("base-os", Name = "GetHomeBaseOs")]
    [ProducesResponseType(typeof(HomeBaseOsDto), StatusCodes.Status200OK, "application/json")]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError, "application/problem+json")]
    public async Task<IActionResult> GetBaseOs(
        [FromQuery] string[]? namespaces,
        [FromQuery] int? limit,
        CancellationToken ctx) =>
        Ok(await homeService.GetBaseOs(ToFilter(null, null, namespaces, limit), ctx));

    [HttpGet("fix-waiting", Name = "GetHomeFixWaiting")]
    [ProducesResponseType(typeof(HomeFixWaitingDto), StatusCodes.Status200OK, "application/json")]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError, "application/problem+json")]
    public async Task<IActionResult> GetFixWaiting(
        [FromQuery] int[]? severities,
        [FromQuery] string[]? namespaces,
        [FromQuery] int? limit,
        CancellationToken ctx) =>
        Ok(await homeService.GetFixWaiting(ToFilter(null, severities, namespaces, limit), ctx));

    [HttpGet("failing-checks", Name = "GetHomeFailingChecks")]
    [ProducesResponseType(typeof(HomeFailingChecksDto), StatusCodes.Status200OK, "application/json")]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError, "application/problem+json")]
    public async Task<IActionResult> GetFailingChecks(
        [FromQuery] int[]? severities,
        [FromQuery] string[]? namespaces,
        [FromQuery] int? limit,
        CancellationToken ctx) =>
        Ok(await homeService.GetFailingChecks(ToFilter(null, severities, namespaces, limit), ctx));

    [HttpGet("oldest-images", Name = "GetHomeOldestImages")]
    [ProducesResponseType(typeof(HomeOldestImagesDto), StatusCodes.Status200OK, "application/json")]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError, "application/problem+json")]
    public async Task<IActionResult> GetOldestImages(
        [FromQuery] int[]? severities,
        [FromQuery] string[]? namespaces,
        [FromQuery] int? limit,
        CancellationToken ctx) =>
        Ok(await homeService.GetOldestImages(ToFilter(null, severities, namespaces, limit), ctx));

    [HttpGet("package-hotspots", Name = "GetHomePackageHotspots")]
    [ProducesResponseType(typeof(HomePackageHotspotsDto), StatusCodes.Status200OK, "application/json")]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError, "application/problem+json")]
    public async Task<IActionResult> GetPackageHotspots(
        [FromQuery] int[]? severities,
        [FromQuery] string[]? namespaces,
        [FromQuery] int? limit,
        CancellationToken ctx) =>
        Ok(await homeService.GetPackageHotspots(ToFilter(null, severities, namespaces, limit), ctx));

    [HttpGet("reach-epss", Name = "GetHomeReachEpss")]
    [ProducesResponseType(typeof(HomeReachEpssDto), StatusCodes.Status200OK, "application/json")]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError, "application/problem+json")]
    public async Task<IActionResult> GetReachEpss(
        [FromQuery] int[]? severities,
        [FromQuery] string[]? namespaces,
        [FromQuery] int? limit,
        CancellationToken ctx) =>
        Ok(await homeService.GetReachEpss(ToFilter(null, severities, namespaces, limit), ctx));

    // "where is" searches: query = what is searched (see HomeSearchDtos for the matching rules)

    [HttpGet("where-is/vulnerability", Name = "GetHomeWhereIsVulnerability")]
    [ProducesResponseType(typeof(HomeWhereIsVulnerabilityDto), StatusCodes.Status200OK, "application/json")]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError, "application/problem+json")]
    public async Task<IActionResult> FindVulnerability(
        [FromQuery] string? query,
        [FromQuery] string[]? namespaces,
        [FromQuery] int? limit,
        CancellationToken ctx) =>
        Ok(await homeSearchService.FindVulnerability(query ?? string.Empty, ToFilter(null, null, namespaces, limit), ctx));

    [HttpGet("where-is/package", Name = "GetHomeWhereIsPackage")]
    [ProducesResponseType(typeof(HomeWhereIsPackageDto), StatusCodes.Status200OK, "application/json")]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError, "application/problem+json")]
    public async Task<IActionResult> FindPackage(
        [FromQuery] string? query,
        [FromQuery] string[]? namespaces,
        [FromQuery] int? limit,
        CancellationToken ctx) =>
        Ok(await homeSearchService.FindPackage(query ?? string.Empty, ToFilter(null, null, namespaces, limit), ctx));

    [HttpGet("where-is/license", Name = "GetHomeWhereIsLicense")]
    [ProducesResponseType(typeof(HomeWhereIsLicenseDto), StatusCodes.Status200OK, "application/json")]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError, "application/problem+json")]
    public async Task<IActionResult> FindLicense(
        [FromQuery] string? query,
        [FromQuery] string[]? namespaces,
        [FromQuery] int? limit,
        CancellationToken ctx) =>
        Ok(await homeSearchService.FindLicense(query ?? string.Empty, ToFilter(null, null, namespaces, limit), ctx));

    [HttpGet("where-is/check", Name = "GetHomeWhereIsCheck")]
    [ProducesResponseType(typeof(HomeWhereIsCheckDto), StatusCodes.Status200OK, "application/json")]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError, "application/problem+json")]
    public async Task<IActionResult> FindCheck(
        [FromQuery] string? query,
        [FromQuery] string[]? namespaces,
        [FromQuery] int? limit,
        CancellationToken ctx) =>
        Ok(await homeSearchService.FindCheck(query ?? string.Empty, ToFilter(null, null, namespaces, limit), ctx));

    private static HomeFilter ToFilter(DateTime? since, int[]? severities, string[]? namespaces, int? limit) =>
        new(
            ToSince(since),
            severities,
            namespaces is null ? null : [.. namespaces.Where(x => !string.IsNullOrWhiteSpace(x)),],
            Math.Clamp(limit ?? DefaultLimit, 1, MaxLimit));

    // the binder gives a local time for a "Z" value and an unspecified one when there is no offset; both mean UTC here
    private static DateTime ToSince(DateTime? since) =>
        since switch
        {
            null => DateTime.UtcNow.AddDays(-1),
            { Kind: DateTimeKind.Local, } value => value.ToUniversalTime(),
            { Kind: DateTimeKind.Unspecified, } value => DateTime.SpecifyKind(value, DateTimeKind.Utc),
            { } value => value,
        };
}
