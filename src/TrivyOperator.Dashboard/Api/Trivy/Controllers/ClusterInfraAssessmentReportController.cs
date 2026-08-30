using Microsoft.AspNetCore.Mvc;
using TrivyOperator.Dashboard.Application.Queries.Trivy.Models;
using TrivyOperator.Dashboard.Application.Queries.Trivy.Services.ClusterInfraAssessmentReports.Abstractions;

namespace TrivyOperator.Dashboard.Api.Trivy.Controllers;

[ApiController]
[Route("api/cluster-infra-assessment-reports")]
public class ClusterInfraAssessmentReportController(
    IClusterInfraAssessmentReportService clusterInfraAssessmentReportService
) : ControllerBase
{
    [HttpGet(Name = "GetClusterInfraAssessmentReportDtos")]
    [ProducesResponseType<IEnumerable<ClusterInfraAssessmentReportDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status500InternalServerError)]
    public async Task<IEnumerable<ClusterInfraAssessmentReportDto>> Get(
        CancellationToken ctx = default) =>
        await clusterInfraAssessmentReportService
            .GetClusterInfraAssessmentReportDtos(ctx);

    [HttpGet("{uid}", Name = "GetClusterInfraAssessmentReportDtoByUid")]
    [ProducesResponseType<ClusterInfraAssessmentReportDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetByUid(
        string uid,
        CancellationToken ctx = default)
    {
        ClusterInfraAssessmentReportDto? result =
            await clusterInfraAssessmentReportService
                .GetClusterInfraAssessmentReportDtoByUid(uid, ctx);

        return result is null ? NotFound() : Ok(result);
    }

    [HttpGet("denormalized", Name = "GetClusterInfraAssessmentReportDenormalizedDtos")]
    [ProducesResponseType<IEnumerable<ClusterInfraAssessmentReportDenormalizedDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status500InternalServerError)]
    public async Task<IEnumerable<ClusterInfraAssessmentReportDenormalizedDto>> GetDenormalized(
        CancellationToken ctx = default) =>
        await clusterInfraAssessmentReportService
            .GetClusterInfraAssessmentReportDenormalizedDtos(ctx);
}