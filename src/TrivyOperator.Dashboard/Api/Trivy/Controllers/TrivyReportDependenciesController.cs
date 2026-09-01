
using Microsoft.AspNetCore.Mvc;
using TrivyOperator.Dashboard.Application.Queries.Trivy.Models;
using TrivyOperator.Dashboard.Application.Queries.TrivyDependencies.Models;
using TrivyOperator.Dashboard.Application.Queries.TrivyDependencies.Services.Abstractions;

namespace TrivyOperator.Dashboard.Api.Trivy.Controllers;

[ApiController]
[Route("api/trivy-report-dependencies")]
public class TrivyReportDependenciesController(ITrivyReportDependenciesService trivyReportDependenciesServiceService)
    : ControllerBase
{
    [HttpGet("digest", Name = "GetTrivyReportDependencyDtoByDigestNamespace")]
    [ProducesResponseType<TrivyDependencyTreeDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetByDigestNamespace([FromQuery] string digest, [FromQuery] string namespaceName, CancellationToken ct)
    {
        TrivyDependencyTreeDto? trivyReportDependencyDto =
            await trivyReportDependenciesServiceService.GetTrivyDependencyTree(digest, namespaceName, ct);

        return trivyReportDependencyDto is null ? NotFound() : Ok(trivyReportDependencyDto);
    }
    
    [HttpGet("digest/check", Name = "CheckIfTrivyDependenciesExistByDigestNamespace")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> CheckIfTrivyDependenciesExist([FromQuery] string digest, [FromQuery] string namespaceName, CancellationToken ct)
    {
        bool result =
            await trivyReportDependenciesServiceService.TrivyDependenciesExist(digest, namespaceName, ct);

        return result ? Ok() : NotFound();
    }
}
