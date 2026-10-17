using Microsoft.AspNetCore.Mvc;
using TrivyOperator.Dashboard.Application.Shared.Queries;
using TrivyOperator.Dashboard.Application.Trivy.Queries.Trivy.Models;
using TrivyOperator.Dashboard.Application.Trivy.Queries.Trivy.Services.ClusterConfigAuditReports.Abstractions;

namespace TrivyOperator.Dashboard.Api.Trivy.Controllers;

[ApiController]
[Route("api/cluster-config-audit-reports")]
public class ClusterConfigAuditReportController(
    IClusterConfigAuditReportService clusterConfigAuditReportService
) : ControllerBase
{
    [HttpGet(Name = "GetClusterConfigAuditReportDtos")]
    [ProducesResponseType(typeof(IEnumerable<ClusterConfigAuditReportDto>), StatusCodes.Status200OK, "application/json")]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest, "application/problem+json")]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError, "application/problem+json")]
    public async Task<IActionResult> Get(
        [FromQuery] string? excludedSeverities,
        CancellationToken ctx = default)
    {
        QueryResponse<IEnumerable<ClusterConfigAuditReportDto>> result =
            await clusterConfigAuditReportService.GetClusterConfigAuditReportDtos(
                excludedSeverities,
                ctx);

        return result.Error is null
            ? Ok(result.Payload)
            : Problem(statusCode: StatusCodes.Status400BadRequest, title: "Invalid request", detail: result.Error);
    }

    [HttpGet("{uid}", Name = "GetClusterConfigAuditReportDtoByUid")]
    [ProducesResponseType(typeof(ClusterConfigAuditReportDto), StatusCodes.Status200OK, "application/json")]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest, "application/problem+json")]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound, "application/problem+json")]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError, "application/problem+json")]
    public async Task<IActionResult> GetByUid(
        string uid,
        CancellationToken ctx = default)
    {
        ClusterConfigAuditReportDto? result =
            await clusterConfigAuditReportService
                .GetClusterConfigAuditReportDtoByUid(uid, ctx);

        return result is null
            ? Problem(statusCode: StatusCodes.Status404NotFound, title: "Cluster config audit report not found")
            : Ok(result);
    }

    [HttpGet("denormalized", Name = "GetClusterConfigAuditReportDenormalizedDtos")]
    [ProducesResponseType(typeof(IEnumerable<ClusterConfigAuditReportDenormalizedDto>), StatusCodes.Status200OK, "application/json")]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError, "application/problem+json")]
    public async Task<IActionResult> GetDenormalized(
        CancellationToken ctx = default) =>
        Ok(await clusterConfigAuditReportService
            .GetClusterConfigAuditReportDenormalizedDtos(ctx));
}
