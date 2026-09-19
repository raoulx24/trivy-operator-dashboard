using Microsoft.AspNetCore.Mvc;
using TrivyOperator.Dashboard.Application.Queries.AppVersions.Models;
using TrivyOperator.Dashboard.Application.Queries.AppVersions.Services.Abstractions;
using TrivyOperator.Dashboard.Domain.Releases.Entities;

namespace TrivyOperator.Dashboard.Api.AppVersions.Controllers;

[ApiController]
[Route("api/app-versions")]
public class AppVersionsController(IAppVersionsService appVersionsService)
{
    [HttpGet(Name = "GetGitHubVersions")]
    [ProducesResponseType<IEnumerable<ReleaseDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status500InternalServerError)]
    public async Task<IEnumerable<ReleaseDto>> GetAll() => await appVersionsService.GetTrivyDashboardReleases();

    [HttpGet("latest", Name = "GetGitHubLatestVersion")]
    [ProducesResponseType<ReleaseDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status500InternalServerError)]
    public async Task<ReleaseDto> GetLatest() =>
        await appVersionsService.GetTrivyDashboardLatestRelease() ?? new Release().ToReleaseDto();

    [HttpGet("current-version", Name = "GetCurrentVersion")]
    [ProducesResponseType<AppVersion>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status500InternalServerError)]
    public AppVersion GetCurrentAppVersion() => appVersionsService.GetCurrentVersion();

    // TODO - proper error handling with Task<IActionResult>
    // TODO - proper version return (and also, in GitHubClient, user agent
}
