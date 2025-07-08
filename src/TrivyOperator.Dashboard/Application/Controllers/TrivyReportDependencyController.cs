﻿using Microsoft.AspNetCore.Mvc;
using TrivyOperator.Dashboard.Application.Models;
using TrivyOperator.Dashboard.Application.Services.TrivyReportDependencies.Abstractions;

namespace TrivyOperator.Dashboard.Application.Controllers;

[ApiController]
[Route("api/trivy-report-dependencies")]
public class TrivyReportDependencyController(ITrivyReportDependency trivyReportDependencyService) : ControllerBase
{
    [HttpGet("digest", Name = "GetTrivyReportDependecyDtoByDigestNamespace")]
    [ProducesResponseType<TrivyReportDependencyDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetByDigestNamespace([FromQuery] string digest, [FromQuery] string namespaceName)
    {
        TrivyReportDependencyDto? trivyReportDependencyDto = await trivyReportDependencyService.GetTryvyReportDependencies(digest, namespaceName);

        return trivyReportDependencyDto is null ? NotFound() : Ok(trivyReportDependencyDto);
    }
}
