using Microsoft.AspNetCore.Mvc;
using TrivyOperator.Dashboard.Application.Trivy.Queries.Severities.Models;
using TrivyOperator.Dashboard.Application.Trivy.Queries.Severities.Services.Abstractions;

namespace TrivyOperator.Dashboard.Api.Trivy.Controllers;

[ApiController]
[Route("api/severities")]
public class SeveritiesController(ISeverityService service) : ControllerBase
{
    [HttpGet(Name = "GetSeverities")]
    [ProducesResponseType(typeof(IEnumerable<SeverityDto>), StatusCodes.Status200OK, "application/json")]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError, "application/problem+json")]
    public async Task<IActionResult> GetAll(CancellationToken ctx) => Ok(await service.GetAll(ctx));
}
