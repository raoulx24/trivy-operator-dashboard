using Microsoft.AspNetCore.Mvc;
using TrivyOperator.Dashboard.Application.AppVersions.Queries.Models;
using TrivyOperator.Dashboard.Application.AppVersions.Queries.Services.Abstractions;
using TrivyOperator.Dashboard.Domain.Releases.Entities;

namespace TrivyOperator.Dashboard.Api.AppVersions.Controllers;

[ApiController]
[Route("api/app-versions")]
public class AppVersionsController(IAppVersionsService appVersionsService) : ControllerBase
{
    [HttpGet(Name = "GetGitHubVersions")]
    [ProducesResponseType(typeof(IEnumerable<ReleaseDto>), StatusCodes.Status200OK, "application/json")]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError, "application/problem+json")]
    public async Task<IActionResult> GetAll() => Ok(await appVersionsService.GetTrivyDashboardReleases());

    [HttpGet("latest", Name = "GetGitHubLatestVersion")]
    [ProducesResponseType(typeof(ReleaseDto), StatusCodes.Status200OK, "application/json")]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError, "application/problem+json")]
    public async Task<IActionResult> GetLatest() =>
        Ok(await appVersionsService.GetTrivyDashboardLatestRelease() ?? new Release().ToReleaseDto());

    [HttpGet("current-version", Name = "GetCurrentVersion")]
    [ProducesResponseType(typeof(AppVersion), StatusCodes.Status200OK, "application/json")]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError, "application/problem+json")]
    public IActionResult GetCurrentAppVersion() => Ok(appVersionsService.GetCurrentVersion());

    // TODO - proper version return (and also, in GitHubClient, user agent
}
