using Microsoft.AspNetCore.Mvc;
using TrivyOperator.Dashboard.Application.Queries.Trivy.Models;
using TrivyOperator.Dashboard.Application.Queries.Trivy.Services.ClusterSbomReports.Abstractions;

namespace TrivyOperator.Dashboard.Api.Controllers.Trivy;

[ApiController]
[Route("api/cluster-sbom-reports")]
public class ClusterSbomReportController(IClusterSbomReportService clusterSbomReportService) : ControllerBase
{
    [HttpGet("minimal", Name = "GetClusterSbomReportMinimalDtos")]
    [ProducesResponseType<IEnumerable<SbomReportImageMinimalDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetGroupedByImageMinimal(CancellationToken ctx) =>
        Ok(await clusterSbomReportService.GetClusterSbomReportMinimalDtos(ctx));
    
    [HttpGet(Name = "GetClusterSbomReportDtos")]
    [ProducesResponseType<IEnumerable<ClusterSbomReportDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status500InternalServerError)]
    public async Task<IEnumerable<ClusterSbomReportDto>> Get() =>
        await clusterSbomReportService.GetClusterSbomReportDtos();

    [HttpGet("denormalized", Name = "GetClusterSbomReportDenormalizedDtos")]
    [ProducesResponseType<IEnumerable<ClusterSbomReportDenormalizedDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status500InternalServerError)]
    public async Task<IEnumerable<ClusterSbomReportDenormalizedDto>> GetDenormalized() =>
        await clusterSbomReportService.GetClusterSbomReportDenormalizedDtos();
}
