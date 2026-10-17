using Microsoft.AspNetCore.Mvc;
using TrivyOperator.Dashboard.Application.Trivy.Queries.Trivy.Models;
using TrivyOperator.Dashboard.Application.Trivy.Queries.Trivy.Services.ClusterComplianceReports.Abstractions;

namespace TrivyOperator.Dashboard.Api.Trivy.Controllers;

[ApiController]
[Route("api/cluster-compliance-reports")]
public class ClusterComplianceReportController(IClusterComplianceReportService clusterComplianceReportService)
    : ControllerBase
{
    [HttpGet(Name = "GetClusterComplianceReportDtos")]
    [ProducesResponseType(typeof(IEnumerable<ClusterComplianceReportDto>), StatusCodes.Status200OK, "application/json")]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError, "application/problem+json")]
    public async Task<IActionResult> Get() =>
        Ok(await clusterComplianceReportService.GetClusterComplianceReportDtos());

    [HttpGet("{uid}", Name = "GetClusterComplianceReportDtoByUid")]
    [ProducesResponseType(typeof(ClusterComplianceReportDto), StatusCodes.Status200OK, "application/json")]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest, "application/problem+json")]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound, "application/problem+json")]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError, "application/problem+json")]
    public async Task<IActionResult> GetByUid(
        string uid,
        CancellationToken ctx = default)
    {
        ClusterComplianceReportDto? result =
            await clusterComplianceReportService
                .GetClusterComplianceReportDtoByUid(uid, ctx);

        return result is null
            ? Problem(statusCode: StatusCodes.Status404NotFound, title: "Cluster compliance report not found")
            : Ok(result);
    }

    [HttpGet("denormalized", Name = "GetClusterComplianceReportDenormalizedDtos")]
    [ProducesResponseType(typeof(IEnumerable<ClusterComplianceReportDenormalizedDto>), StatusCodes.Status200OK, "application/json")]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError, "application/problem+json")]
    public async Task<IActionResult> GetDenormalized() =>
        Ok(await clusterComplianceReportService.GetClusterComplianceReportDenormalizedDtos());
}
