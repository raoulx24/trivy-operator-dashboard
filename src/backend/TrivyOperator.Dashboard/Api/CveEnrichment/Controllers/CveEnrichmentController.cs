using Microsoft.AspNetCore.Mvc;
using TrivyOperator.Dashboard.Application.CveEnrichment.Queries.Models;
using TrivyOperator.Dashboard.Application.CveEnrichment.Queries.Services.Abstractions;

namespace TrivyOperator.Dashboard.Api.CveEnrichment.Controllers;

[ApiController]
[Route("api/cve-enrichment")]
public class CveEnrichmentController(ICveEnrichmentService cveEnrichmentService) : ControllerBase
{
    [HttpGet(Name = "GetCveEnrichments")]
    [ProducesResponseType(typeof(IReadOnlyList<CveInfoDto>), StatusCodes.Status200OK, "application/json")]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError, "application/problem+json")]
    public async Task<IActionResult> GetAll(CancellationToken ctx) =>
        Ok(await cveEnrichmentService.GetAll(ctx));

    [HttpGet("{cveId}", Name = "GetCveEnrichmentById")]
    [ProducesResponseType(typeof(CveInfoDto), StatusCodes.Status200OK, "application/json")]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError, "application/problem+json")]
    public async Task<IActionResult> GetById(string cveId, CancellationToken ctx)
    {
        CveInfoDto? result = await cveEnrichmentService.GetById(cveId, ctx);

        return result is null ? NotFound() : Ok(result);
    }

    [HttpPost("rebuild", Name = "RebuildCveEnrichment")]
    [ProducesResponseType(StatusCodes.Status202Accepted)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError, "application/problem+json")]
    public IActionResult Rebuild()
    {
        cveEnrichmentService.RequestRebuild();

        return Accepted();
    }
}
