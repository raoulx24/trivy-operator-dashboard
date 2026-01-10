using Microsoft.AspNetCore.Mvc;
using TrivyOperator.Dashboard.Application.Trivy.Models;
using TrivyOperator.Dashboard.Application.Trivy.Services.ClusterInfraAssessmentReport.Abstractions;

namespace TrivyOperator.Dashboard.Application.Trivy.Controllers;

[ApiController]
[Route("api/cluster-infra-assessment-reports")]
public class ClusterInfraAssessmentReportController(IClusterInfraAssessmentReportService clusterInfraAssessmentReportService) : ControllerBase
{
    [HttpGet(Name = "GetClusterInfraAssessmentReportDtos")]
    [ProducesResponseType<IEnumerable<ClusterInfraAssessmentReportDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status500InternalServerError)]
    public async Task<IResult> Get()
    {
        IEnumerable<ClusterInfraAssessmentReportDto> clusterInfraAssessmentReportImageDtos =
            await clusterInfraAssessmentReportService.GetClusterInfraAssessmentReportDtos();

        return Results.Ok(clusterInfraAssessmentReportImageDtos);
    }


    [HttpGet("{uid:guid}", Name = "GetClusterInfraAssessmentReportDtoByUid")]
    [ProducesResponseType<ClusterInfraAssessmentReportDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status500InternalServerError)]
    public async Task<IResult> GetByUid(Guid uid)
    {
        ClusterInfraAssessmentReportDto? clusterInfraAssessmentReportDto =
            await clusterInfraAssessmentReportService.GetClusterInfraAssessmentReportDtoByUid(uid);

        return clusterInfraAssessmentReportDto is null
            ? Results.NotFound()
            : Results.Ok(clusterInfraAssessmentReportDto);
    }


    [HttpGet("denormalized", Name = "GetClusterInfraAssessmentReportDenormalizedDtos")]
    [ProducesResponseType<IEnumerable<ClusterInfraAssessmentReportDenormalizedDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status500InternalServerError)]
    public async Task<IEnumerable<ClusterInfraAssessmentReportDenormalizedDto>> GetDenormalized() =>
        await clusterInfraAssessmentReportService.GetClusterInfraAssessmentReportDenormalizedDtos();
}
