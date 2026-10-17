using Microsoft.AspNetCore.Mvc;
using TrivyOperator.Dashboard.Application.Trivy.Queries.Trivy.Models;
using TrivyOperator.Dashboard.Application.Trivy.Queries.Trivy.Services.ClusterInfraAssessmentReports.Abstractions;

namespace TrivyOperator.Dashboard.Api.Trivy.Controllers;

[ApiController]
[Route("api/cluster-infra-assessment-reports")]
public class ClusterInfraAssessmentReportController(
    IClusterInfraAssessmentReportService clusterInfraAssessmentReportService
) : ControllerBase
{
    [HttpGet(Name = "GetClusterInfraAssessmentReportDtos")]
    [ProducesResponseType(typeof(IEnumerable<ClusterInfraAssessmentReportDto>), StatusCodes.Status200OK, "application/json")]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError, "application/problem+json")]
    public async Task<IActionResult> Get(
        CancellationToken ctx = default) =>
        Ok(await clusterInfraAssessmentReportService
            .GetClusterInfraAssessmentReportDtos(ctx));

    [HttpGet("{uid}", Name = "GetClusterInfraAssessmentReportDtoByUid")]
    [ProducesResponseType(typeof(ClusterInfraAssessmentReportDto), StatusCodes.Status200OK, "application/json")]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest, "application/problem+json")]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound, "application/problem+json")]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError, "application/problem+json")]
    public async Task<IActionResult> GetByUid(
        string uid,
        CancellationToken ctx = default)
    {
        ClusterInfraAssessmentReportDto? result =
            await clusterInfraAssessmentReportService
                .GetClusterInfraAssessmentReportDtoByUid(uid, ctx);

        return result is null
            ? Problem(statusCode: StatusCodes.Status404NotFound, title: "Cluster infra assessment report not found")
            : Ok(result);
    }

    [HttpGet("denormalized", Name = "GetClusterInfraAssessmentReportDenormalizedDtos")]
    [ProducesResponseType(typeof(IEnumerable<ClusterInfraAssessmentReportDenormalizedDto>), StatusCodes.Status200OK, "application/json")]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError, "application/problem+json")]
    public async Task<IActionResult> GetDenormalized(
        CancellationToken ctx = default) =>
        Ok(await clusterInfraAssessmentReportService
            .GetClusterInfraAssessmentReportDenormalizedDtos(ctx));
}
