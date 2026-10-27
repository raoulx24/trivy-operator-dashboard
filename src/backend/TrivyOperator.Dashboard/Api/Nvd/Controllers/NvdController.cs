using Microsoft.AspNetCore.Mvc;
using TrivyOperator.Dashboard.Application.Nvd.Queries.Models;
using TrivyOperator.Dashboard.Application.Nvd.Queries.Services.Abstractions;

namespace TrivyOperator.Dashboard.Api.Nvd.Controllers;

[ApiController]
[Route("api/nvd")]
public class NvdController(INvdService nvdService) : ControllerBase
{
    // 404 - not a CVE, not in the current vulnerability reports, or NVD is disabled
    // 200 - also for NotListed and Error (see status and errorMessage)
    [HttpGet("{cveId}", Name = "GetNvdCveById")]
    [ProducesResponseType(typeof(NvdCveDto), StatusCodes.Status200OK, "application/json")]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError, "application/problem+json")]
    public async Task<IActionResult> GetById(string cveId, CancellationToken ctx)
    {
        NvdCveDto? result = await nvdService.GetById(cveId, ctx);

        return result is null ? NotFound() : Ok(result);
    }
}
