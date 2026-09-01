using Microsoft.AspNetCore.Mvc;
using TrivyOperator.Dashboard.Application.Queries.Trivy.Models;
using TrivyOperator.Dashboard.Application.Queries.Trivy.Services.ClusterRbacAssessmentReports.Abstractions;

namespace TrivyOperator.Dashboard.Api.Controllers.Trivy;

[ApiController]
[Route("api/cluster-rbac-assessment-reports")]
public class ClusterRbacAssessmentReportController(
    IClusterRbacAssessmentReportService clusterRbacAssessmentReportService
) : ControllerBase
{
    [HttpGet(Name = "GetClusterRbacAssessmentReportDtos")]
    [ProducesResponseType<IEnumerable<ClusterRbacAssessmentReportDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status500InternalServerError)]
    public async Task<IEnumerable<ClusterRbacAssessmentReportDto>> Get(
        CancellationToken ctx = default) =>
        await clusterRbacAssessmentReportService
            .GetClusterRbacAssessmentReportDtos(ctx);
    
    [HttpGet("{uid}", Name = "GetClusterRbacAssessmentReportDtoByUid")]
    [ProducesResponseType<ClusterRbacAssessmentReportDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetByUid(
        string uid,
        CancellationToken ctx = default)
    {
        ClusterRbacAssessmentReportDto? result =
            await clusterRbacAssessmentReportService
                .GetClusterRbacAssessmentReportDtoByUid(uid, ctx);

        return result is null ? NotFound() : Ok(result);
    }

    [HttpGet("denormalized", Name = "GetClusterRbacAssessmentReportDenormalizedDtos")]
    [ProducesResponseType<IEnumerable<ClusterRbacAssessmentReportDenormalizedDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status500InternalServerError)]
    public async Task<IEnumerable<ClusterRbacAssessmentReportDenormalizedDto>> GetDenormalized(
        CancellationToken ctx = default) =>
        await clusterRbacAssessmentReportService
            .GetClusterRbacAssessmentReportDenormalizedDtos(ctx);
}
