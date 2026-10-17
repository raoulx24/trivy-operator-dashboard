using Microsoft.AspNetCore.Mvc;
using TrivyOperator.Dashboard.Application.Trivy.Queries.Trivy.Models;
using TrivyOperator.Dashboard.Application.Trivy.Queries.Trivy.Services.ClusterRbacAssessmentReports.Abstractions;

namespace TrivyOperator.Dashboard.Api.Trivy.Controllers;

[ApiController]
[Route("api/cluster-rbac-assessment-reports")]
public class ClusterRbacAssessmentReportController(
    IClusterRbacAssessmentReportService clusterRbacAssessmentReportService
) : ControllerBase
{
    [HttpGet(Name = "GetClusterRbacAssessmentReportDtos")]
    [ProducesResponseType(typeof(IEnumerable<ClusterRbacAssessmentReportDto>), StatusCodes.Status200OK, "application/json")]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError, "application/problem+json")]
    public async Task<IActionResult> Get(
        CancellationToken ctx = default) =>
        Ok(await clusterRbacAssessmentReportService
            .GetClusterRbacAssessmentReportDtos(ctx));

    [HttpGet("{uid}", Name = "GetClusterRbacAssessmentReportDtoByUid")]
    [ProducesResponseType(typeof(ClusterRbacAssessmentReportDto), StatusCodes.Status200OK, "application/json")]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest, "application/problem+json")]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound, "application/problem+json")]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError, "application/problem+json")]
    public async Task<IActionResult> GetByUid(
        string uid,
        CancellationToken ctx = default)
    {
        ClusterRbacAssessmentReportDto? result =
            await clusterRbacAssessmentReportService
                .GetClusterRbacAssessmentReportDtoByUid(uid, ctx);

        return result is null
            ? Problem(statusCode: StatusCodes.Status404NotFound, title: "Cluster RBAC assessment report not found")
            : Ok(result);
    }

    [HttpGet("denormalized", Name = "GetClusterRbacAssessmentReportDenormalizedDtos")]
    [ProducesResponseType(typeof(IEnumerable<ClusterRbacAssessmentReportDenormalizedDto>), StatusCodes.Status200OK, "application/json")]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError, "application/problem+json")]
    public async Task<IActionResult> GetDenormalized(
        CancellationToken ctx = default) =>
        Ok(await clusterRbacAssessmentReportService
            .GetClusterRbacAssessmentReportDenormalizedDtos(ctx));
}
