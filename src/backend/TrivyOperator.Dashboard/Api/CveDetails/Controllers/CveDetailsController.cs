using Microsoft.AspNetCore.Mvc;
using TrivyOperator.Dashboard.Application.CveDetails.Queries.Models;
using TrivyOperator.Dashboard.Application.CveDetails.Queries.Services.Abstractions;

namespace TrivyOperator.Dashboard.Api.CveDetails.Controllers;

[ApiController]
[Route("api/cve-details")]
public class CveDetailsController(ICveDetailsService cveDetailsService) : ControllerBase
{
    // 200 - also when the provider could not be asked (rate limited, timeout...) - status Error, see errorMessage
    // 400 - not a CVE id
    // 404 - not in the current vulnerability reports, unknown to the provider, or CVE details lookups disabled (see detail)
    [HttpGet("{cveId}", Name = "GetCveDetailsById")]
    [ProducesResponseType(typeof(CveDetailsDto), StatusCodes.Status200OK, "application/json")]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest, "application/problem+json")]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound, "application/problem+json")]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError, "application/problem+json")]
    public async Task<IActionResult> GetById(string cveId, CancellationToken ctx)
    {
        CveDetailsResult result = await cveDetailsService.GetById(cveId, ctx);
        CveDetailsLookupOutcome outcome = result.Outcome;

        if (outcome == CveDetailsLookupOutcomes.Found && result.Cve is not null)
        {
            return Ok(result.Cve);
        }

        if (outcome == CveDetailsLookupOutcomes.InvalidId)
        {
            return Problem(
                statusCode: StatusCodes.Status400BadRequest,
                title: "Invalid CVE id",
                detail: "Expected a CVE id - CVE-YYYY-NNNN (4 to 19 digits).");
        }

        if (outcome == CveDetailsLookupOutcomes.NotSubject)
        {
            return Problem(
                statusCode: StatusCodes.Status404NotFound,
                title: "CVE not found",
                detail: $"{cveId.Trim().ToUpperInvariant()} is not in the current vulnerability reports.");
        }

        if (outcome == CveDetailsLookupOutcomes.NotListed)
        {
            return Problem(
                statusCode: StatusCodes.Status404NotFound,
                title: "CVE details not found",
                detail: $"The CVE details provider has no record of {cveId.Trim().ToUpperInvariant()} (yet).");
        }

        if (outcome == CveDetailsLookupOutcomes.Disabled)
        {
            return Problem(
                statusCode: StatusCodes.Status404NotFound,
                title: "CVE details lookups are disabled",
                detail: "CVE details lookups are disabled, or no vulnerability reports are enabled.");
        }

        return Problem(
            statusCode: StatusCodes.Status500InternalServerError,
            title: "Unexpected CVE details lookup outcome",
            detail: outcome.Value);
    }
}
