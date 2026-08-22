using Microsoft.AspNetCore.Mvc;
using TrivyOperator.Dashboard.Application.Trivy.Models;
using TrivyOperator.Dashboard.Domain.Trivy.ValueObjects.Shared;

namespace TrivyOperator.Dashboard.Application.Trivy.Controllers;

[ApiController]
[Route("api/severities")]
public class SeveritiesController : ControllerBase
{
    [HttpGet(Name = "GetSeverities")]
    [ProducesResponseType<IEnumerable<SeverityDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status500InternalServerError)]
    public Task<IEnumerable<SeverityDto>> GetAll()
    {
        IEnumerable<SeverityDto> result = Severity.RankedSeverities.Select(severity =>
            new SeverityDto()
            {
                Id = severity.Rank,
                Name = severity.Value,
            }
        );

        return Task.FromResult(result);
    }
}
