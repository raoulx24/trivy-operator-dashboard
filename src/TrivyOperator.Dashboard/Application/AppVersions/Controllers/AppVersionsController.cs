using Microsoft.AspNetCore.Mvc;
using TrivyOperator.Dashboard.Application.AppVersions.Models;
using TrivyOperator.Dashboard.Application.AppVersions.Services.Abstractions;

namespace TrivyOperator.Dashboard.Application.AppVersions.Controllers;

[ApiController]
[Route("api/app-versions")]
public class AppVersionsController(IAppVersionService appVersionService)
{
    [HttpGet(Name = "GetGitHubVersions")]
    [ProducesResponseType<IEnumerable<GitHubReleaseDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status500InternalServerError)]
    public async Task<IEnumerable<GitHubReleaseDto>> GetAll()
    {
        return await appVersionService.GetTrivyDashboardReleases();
    }

    [HttpGet("latest", Name = "GetGitHubLatestVersion")]
    [ProducesResponseType<GitHubReleaseDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status500InternalServerError)]
    public async Task<GitHubReleaseDto> GetLatest()
    {
        return await appVersionService.GetTrivyDashboardLatestRelease() ?? new GitHubReleaseDto();
    }

    [HttpGet("current-version", Name = "GetCurrentVersion")]
    [ProducesResponseType<AppVersion>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status500InternalServerError)]
    public AppVersion GetCurrentAppVersion()
    {
        return appVersionService.GetCurrentVersion();
    }

    // TODO - proper error handling with Task<IActionResult>
    // TODO - proper version return (and also, in GitHubClient, user agent
}
