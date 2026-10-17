using Microsoft.AspNetCore.Mvc;
using TrivyOperator.Dashboard.Application.TrivyDependencies.Queries.Models;
using TrivyOperator.Dashboard.Application.TrivyDependencies.Queries.Services.Abstractions;

namespace TrivyOperator.Dashboard.Api.Trivy.Controllers;

[ApiController]
[Route("api/trivy-report-dependencies")]
public class TrivyReportDependenciesController(ITrivyReportDependenciesService trivyReportDependenciesServiceService)
    : ControllerBase
{
    [HttpGet("digest", Name = "GetTrivyReportDependencyDtoByDigestNamespace")]
    [ProducesResponseType(typeof(TrivyDependencyTreeDto), StatusCodes.Status200OK, "application/json")]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest, "application/problem+json")]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound, "application/problem+json")]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError, "application/problem+json")]
    public async Task<IActionResult> GetByDigestNamespace([FromQuery] string digest, [FromQuery] string? namespaceName, CancellationToken ct)
    {
        TrivyDependencyTreeDto? trivyReportDependencyDto =
            await trivyReportDependenciesServiceService.GetTrivyDependencyTree(digest, namespaceName, ct);

        return trivyReportDependencyDto is null
            ? Problem(statusCode: StatusCodes.Status404NotFound, title: "Trivy dependencies not found")
            : Ok(trivyReportDependencyDto);
    }

    [HttpGet("digest/check", Name = "CheckIfTrivyDependenciesExistByDigestNamespace")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest, "application/problem+json")]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound, "application/problem+json")]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError, "application/problem+json")]
    public async Task<IActionResult> CheckIfTrivyDependenciesExist([FromQuery] string digest, CancellationToken ct)
    {
        bool result =
            await trivyReportDependenciesServiceService.TrivyDependenciesExist(digest);

        return result
            ? Ok()
            : Problem(statusCode: StatusCodes.Status404NotFound, title: "Trivy dependencies not found");
    }
}
