using Microsoft.AspNetCore.Mvc;
using TrivyOperator.Dashboard.Application.Shared.Queries;
using TrivyOperator.Dashboard.Application.Trivy.Queries.Trivy.Models;
using TrivyOperator.Dashboard.Application.Trivy.Queries.Trivy.Services.ConfigAuditReports.Abstractions;

namespace TrivyOperator.Dashboard.Api.Trivy.Controllers;

[ApiController]
[Route("api/config-audit-reports")]
public class ConfigAuditReportController(
    IConfigAuditReportService configAuditReportService
) : ControllerBase
{
    [HttpGet(Name = "GetConfigAuditReportDtos")]
    [ProducesResponseType(typeof(IEnumerable<ConfigAuditReportDto>), StatusCodes.Status200OK, "application/json")]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest, "application/problem+json")]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError, "application/problem+json")]
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
            : Problem(statusCode: StatusCodes.Status400BadRequest, title: "Invalid request", detail: result.Error);
    }

    [HttpGet("{uid}", Name = "GetConfigAuditReportDtoByUid")]
    [ProducesResponseType(typeof(ConfigAuditReportDto), StatusCodes.Status200OK, "application/json")]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest, "application/problem+json")]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound, "application/problem+json")]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError, "application/problem+json")]
    public async Task<IActionResult> GetByUid(
        string uid,
        CancellationToken ctx = default)
    {
        ConfigAuditReportDto? result =
            await configAuditReportService.GetConfigAuditReportDtoByUid(uid, ctx);

        return result is null
            ? Problem(statusCode: StatusCodes.Status404NotFound, title: "Config audit report not found")
            : Ok(result);
    }

    [HttpGet("denormalized", Name = "GetConfigAuditReportDenormalizedDtos")]
    [ProducesResponseType(typeof(IEnumerable<ConfigAuditReportDenormalizedDto>), StatusCodes.Status200OK, "application/json")]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError, "application/problem+json")]
    public async Task<IActionResult> GetDenormalized(
        [FromQuery] string? namespaceName,
        CancellationToken ctx = default) =>
        Ok(await configAuditReportService.GetConfigAuditReportDenormalizedDtos(
            namespaceName,
            ctx));

    [HttpGet("active-namespaces", Name = "GetConfigAuditReportActiveNamespaces")]
    [ProducesResponseType(typeof(string[]), StatusCodes.Status200OK, "application/json")]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError, "application/problem+json")]
    public async Task<IActionResult> GetActiveNamespaces(CancellationToken ctx) =>
        Ok(await configAuditReportService.GetActiveNamespaces(ctx));
}
