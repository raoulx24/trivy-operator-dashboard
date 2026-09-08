using Microsoft.AspNetCore.Mvc;
using TrivyOperator.Dashboard.Application.Queries.Shared;
using TrivyOperator.Dashboard.Application.Queries.Trivy.Models;
using TrivyOperator.Dashboard.Application.Queries.Trivy.Services.ConfigAuditReports.Abstractions;

namespace TrivyOperator.Dashboard.Api.Trivy.Controllers;

[ApiController]
[Route("api/config-audit-reports")]
public class ConfigAuditReportController(
    IConfigAuditReportService configAuditReportService
) : ControllerBase
{
    [HttpGet(Name = "GetConfigAuditReportDtos")]
    [ProducesResponseType<IEnumerable<ConfigAuditReportDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Get(
        [FromQuery] string? namespaceName,
        [FromQuery] string? excludedSeverities,
        CancellationToken ctx = default)
    {
        QueryResponse<IEnumerable<ConfigAuditReportDto>> result =
            await configAuditReportService.GetConfigAuditReportDtos(
                namespaceName,
                excludedSeverities,
                ctx);

        return result.Error is null
            ? Ok(result.Payload)
            : BadRequest(result.Error);
    }

    [HttpGet("{uid}", Name = "GetConfigAuditReportDtoByUid")]
    [ProducesResponseType<ConfigAuditReportDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetByUid(
        string uid,
        CancellationToken ctx = default)
    {
        ConfigAuditReportDto? result =
            await configAuditReportService.GetConfigAuditReportDtoByUid(uid, ctx);

        return result is null
            ? NotFound()
            : Ok(result);
    }

    [HttpGet("denormalized", Name = "GetConfigAuditReportDenormalizedDtos")]
    [ProducesResponseType<IEnumerable<ConfigAuditReportDenormalizedDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status500InternalServerError)]
    public async Task<IEnumerable<ConfigAuditReportDenormalizedDto>> GetDenormalized(
        [FromQuery] string? namespaceName,
        CancellationToken ctx = default) =>
        await configAuditReportService.GetConfigAuditReportDenormalizedDtos(
            namespaceName,
            ctx);
}
