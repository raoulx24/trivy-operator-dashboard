using Microsoft.AspNetCore.Mvc;
using TrivyOperator.Dashboard.Application.Queries.Shared;
using TrivyOperator.Dashboard.Application.Queries.Trivy.Models;
using TrivyOperator.Dashboard.Application.Queries.Trivy.Services.ClusterConfigAuditReports.Abstractions;

namespace TrivyOperator.Dashboard.Api.Trivy.Controllers;

[ApiController]
[Route("api/cluster-config-audit-reports")]
public class ClusterConfigAuditReportController(
IClusterConfigAuditReportService clusterConfigAuditReportService
) : ControllerBase
{
    
    [HttpGet(Name = "GetClusterConfigAuditReportDtos")]
    [ProducesResponseType<IEnumerable<ClusterConfigAuditReportDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status500InternalServerError)]
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
            : BadRequest(result.Error);
    }

    [HttpGet("{uid}", Name = "GetClusterConfigAuditReportDtoByUid")]
    [ProducesResponseType<ClusterConfigAuditReportDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetByUid(
        string uid,
        CancellationToken ctx = default)
    {
        ClusterConfigAuditReportDto? result =
            await clusterConfigAuditReportService
                .GetClusterConfigAuditReportDtoByUid(uid, ctx);

        return result is null
            ? NotFound()
            : Ok(result);
    }

    [HttpGet("denormalized", Name = "GetClusterConfigAuditReportDenormalizedDtos")]
    [ProducesResponseType<IEnumerable<ClusterConfigAuditReportDenormalizedDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status500InternalServerError)]
    public async Task<IEnumerable<ClusterConfigAuditReportDenormalizedDto>> GetDenormalized(
        CancellationToken ctx = default) =>
        await clusterConfigAuditReportService
            .GetClusterConfigAuditReportDenormalizedDtos(ctx);

}
