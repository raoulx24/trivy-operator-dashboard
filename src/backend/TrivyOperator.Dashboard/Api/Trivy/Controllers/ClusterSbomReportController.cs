using Microsoft.AspNetCore.Mvc;
using TrivyOperator.Dashboard.Application.Trivy.Queries.Trivy.Models;
using TrivyOperator.Dashboard.Application.Trivy.Queries.Trivy.Services.ClusterSbomReports.Abstractions;

namespace TrivyOperator.Dashboard.Api.Trivy.Controllers;

[ApiController]
[Route("api/cluster-sbom-reports")]
public class ClusterSbomReportController(IClusterSbomReportService clusterSbomReportService) : ControllerBase
{
    [HttpGet("minimal", Name = "GetClusterSbomReportMinimalDtos")]
    [ProducesResponseType(typeof(IEnumerable<SbomReportImageMinimalDto>), StatusCodes.Status200OK, "application/json")]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError, "application/problem+json")]
    public async Task<IActionResult> GetGroupedByImageMinimal(CancellationToken ctx) =>
        Ok(await clusterSbomReportService.GetClusterSbomReportMinimalDtos(ctx));

    [HttpGet(Name = "GetClusterSbomReportDtos")]
    [ProducesResponseType(typeof(IEnumerable<ClusterSbomReportDto>), StatusCodes.Status200OK, "application/json")]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError, "application/problem+json")]
    public async Task<IActionResult> Get() =>
        Ok(await clusterSbomReportService.GetClusterSbomReportDtos());

    [HttpGet("denormalized", Name = "GetClusterSbomReportDenormalizedDtos")]
    [ProducesResponseType(typeof(IEnumerable<ClusterSbomReportDenormalizedDto>), StatusCodes.Status200OK, "application/json")]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError, "application/problem+json")]
    public async Task<IActionResult> GetDenormalized() =>
        Ok(await clusterSbomReportService.GetClusterSbomReportDenormalizedDtos());
}
