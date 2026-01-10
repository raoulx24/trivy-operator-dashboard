using Microsoft.AspNetCore.Mvc;
using TrivyOperator.Dashboard.Application.Trivy.Models;
using TrivyOperator.Dashboard.Application.Trivy.Services.ClusterRbacAssessmentReport.Abstractions;

namespace TrivyOperator.Dashboard.Application.Trivy.Controllers;

[ApiController]
[Route("api/cluster-rbac-assessment-reports")]
public class ClusterRbacAssessmentReportController(
    IClusterRbacAssessmentReportService clusterRbacAssessmentReportService) : ControllerBase
{
    [HttpGet(Name = "GetClusterRbacAssessmentReportDtos")]
    [ProducesResponseType<IEnumerable<ClusterRbacAssessmentReportDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status500InternalServerError)]
    public async Task<IEnumerable<ClusterRbacAssessmentReportDto>> Get() =>
        await clusterRbacAssessmentReportService.GetClusterRbacAssessmentReportDtos();

    [HttpGet("denormalized", Name = "GetClusterRbacAssessmentReportDenormalizedDtos")]
    [ProducesResponseType<IEnumerable<ClusterRbacAssessmentReportDenormalizedDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status500InternalServerError)]
    public async Task<IEnumerable<ClusterRbacAssessmentReportDenormalizedDto>> GetDenormalized() =>
        await clusterRbacAssessmentReportService.GetClusterRbacAssessmentReportDenormalizedDtos();

    [HttpGet("summary", Name = "GetClusterRbacAssessmentReportSummaryDtos")]
    [ProducesResponseType<IEnumerable<ClusterRbacAssessmentReportSummaryDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status500InternalServerError)]
    public async Task<IEnumerable<ClusterRbacAssessmentReportSummaryDto>> GetClusterRbacAssessmentReportSummaryDtos() =>
        await clusterRbacAssessmentReportService.GetClusterRbacAssessmentReportSummaryDtos();
}
