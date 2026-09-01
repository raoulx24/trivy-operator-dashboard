using Microsoft.AspNetCore.Mvc;
using TrivyOperator.Dashboard.Application.Queries.Severities.Models;
using TrivyOperator.Dashboard.Application.Queries.Severities.Services.Abstractions;

namespace TrivyOperator.Dashboard.Api.Controllers.Severities;

[ApiController]
[Route("api/severities")]
public class SeveritiesController(ISeverityService service) : ControllerBase
{
    [HttpGet(Name = "GetSeverities")]
    [ProducesResponseType<IEnumerable<SeverityDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status500InternalServerError)]
    public async Task<IEnumerable<SeverityDto>> GetAll(CancellationToken ctx)
    {
        return await service.GetAll(ctx);
    }
}
