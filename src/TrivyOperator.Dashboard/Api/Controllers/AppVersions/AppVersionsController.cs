using Microsoft.AspNetCore.Mvc;
using TrivyOperator.Dashboard.Application.Queries.AppVersions.Models;
using TrivyOperator.Dashboard.Application.Queries.AppVersions.Services.Abstractions;

namespace TrivyOperator.Dashboard.Api.Controllers.AppVersions;

[ApiController]
[Route("api/app-versions")]
public class AppVersionsController(IAppVersionsService appVersionsService)
{
    [HttpGet(Name = "GetGitHubVersions")]
    [ProducesResponseType<IEnumerable<GitHubReleaseDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status500InternalServerError)]
    public async Task<IEnumerable<GitHubReleaseDto>> GetAll() => await appVersionsService.GetTrivyDashboardReleases();

    [HttpGet("latest", Name = "GetGitHubLatestVersion")]
    [ProducesResponseType<GitHubReleaseDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status500InternalServerError)]
    public async Task<GitHubReleaseDto> GetLatest() =>
        await appVersionsService.GetTrivyDashboardLatestRelease() ?? new GitHubReleaseDto();

    [HttpGet("current-version", Name = "GetCurrentVersion")]
    [ProducesResponseType<AppVersion>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status500InternalServerError)]
    public AppVersion GetCurrentAppVersion() => appVersionsService.GetCurrentVersion();

    // TODO - proper error handling with Task<IActionResult>
    // TODO - proper version return (and also, in GitHubClient, user agent
}
