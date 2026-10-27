using Microsoft.AspNetCore.Mvc;
using TrivyOperator.Dashboard.Application.Nvd.Queries.Models;
using TrivyOperator.Dashboard.Application.Nvd.Queries.Services.Abstractions;

namespace TrivyOperator.Dashboard.Api.Nvd.Controllers;

[ApiController]
[Route("api/nvd")]
public class NvdController(INvdService nvdService) : ControllerBase
{
    // 200 - also when NVD could not be asked (rate limited, timeout...) - status Error, see errorMessage
    // 400 - not a CVE id
    // 404 - not in the current vulnerability reports, unknown to NVD, or NVD lookups disabled (see detail)
    [HttpGet("{cveId}", Name = "GetNvdCveById")]
    [ProducesResponseType(typeof(NvdCveDto), StatusCodes.Status200OK, "application/json")]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest, "application/problem+json")]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound, "application/problem+json")]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError, "application/problem+json")]
    public async Task<IActionResult> GetById(string cveId, CancellationToken ctx)
    {
        NvdCveResult result = await nvdService.GetById(cveId, ctx);
        NvdLookupOutcome outcome = result.Outcome;

        if (outcome == NvdLookupOutcomes.Found && result.Cve is not null)
        {
            return Ok(result.Cve);
        }

        if (outcome == NvdLookupOutcomes.InvalidId)
        {
            return Problem(
                statusCode: StatusCodes.Status400BadRequest,
                title: "Invalid CVE id",
                detail: "Expected a CVE id - CVE-YYYY-NNNN (4 to 19 digits).");
        }

        if (outcome == NvdLookupOutcomes.NotSubject)
        {
            return Problem(
                statusCode: StatusCodes.Status404NotFound,
                title: "CVE not found",
                detail: $"{cveId.Trim().ToUpperInvariant()} is not in the current vulnerability reports.");
        }

        if (outcome == NvdLookupOutcomes.NotInNvd)
        {
            return Problem(
                statusCode: StatusCodes.Status404NotFound,
                title: "CVE not found in NVD",
                detail: $"NVD has no record of {cveId.Trim().ToUpperInvariant()} (yet).");
        }

        if (outcome == NvdLookupOutcomes.Disabled)
        {
            return Problem(
                statusCode: StatusCodes.Status404NotFound,
                title: "NVD lookups are disabled",
                detail: "NVD lookups are disabled, or no vulnerability reports are enabled.");
        }

        return Problem(
            statusCode: StatusCodes.Status500InternalServerError,
            title: "Unexpected NVD lookup outcome",
            detail: outcome.Value);
    }
}
