using Microsoft.AspNetCore.Mvc;
using TrivyOperator.Dashboard.Application.Queries.Trivy.Models;
using TrivyOperator.Dashboard.Application.Queries.Trivy.Services.ClusterRbacAssessmentReports.Abstractions;

namespace TrivyOperator.Dashboard.Api.Trivy.Controllers;

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

    [HttpGet("denormalized", Name = "GetClusterRbacAssessmentReportDenormalizedDtos")]
    [ProducesResponseType<IEnumerable<ClusterRbacAssessmentReportDenormalizedDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status500InternalServerError)]
    public async Task<IEnumerable<ClusterRbacAssessmentReportDenormalizedDto>> GetDenormalized(
        CancellationToken ctx = default) =>
        await clusterRbacAssessmentReportService
            .GetClusterRbacAssessmentReportDenormalizedDtos(ctx);
}
