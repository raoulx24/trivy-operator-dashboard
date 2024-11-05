﻿using Microsoft.AspNetCore.Mvc;
using TrivyOperator.Dashboard.Application.Models;
using TrivyOperator.Dashboard.Domain.Trivy;

namespace TrivyOperator.Dashboard.Application.Controllers;

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
        List<SeverityDto> severityDtos = [];
        foreach (int severityId in Enum.GetValues(typeof(TrivySeverity)))
        {
            SeverityDto severityDto = new() { Id = severityId, Name = ((TrivySeverity)severityId).ToString() };
            severityDtos.Add(severityDto);
        }

        return Task.FromResult((IEnumerable<SeverityDto>)severityDtos);
    }
}
