using Microsoft.AspNetCore.Mvc;
using TrivyOperator.Dashboard.Application.TrivyReportDependencies.Models;
using TrivyOperator.Dashboard.Application.TrivyReportDependencies.Services.Abstractions;

namespace TrivyOperator.Dashboard.Application.TrivyReportDependencies.Controllers;

[ApiController]
[Route("api/trivy-report-dependencies")]
public class TrivyReportDependenciesController(ITrivyReportDependenciesService trivyReportDependenciesServiceService) : ControllerBase
{
    [HttpGet("digest", Name = "GetTrivyReportDependecyDtoByDigestNamespace")]
    [ProducesResponseType<TrivyReportDependencyDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetByDigestNamespace([FromQuery] string digest, [FromQuery] string namespaceName)
    {
        TrivyReportDependencyDto? trivyReportDependencyDto = await trivyReportDependenciesServiceService.GetTryvyReportDependencies(digest, namespaceName);

        return trivyReportDependencyDto is null ? NotFound() : Ok(trivyReportDependencyDto);
    }
}
