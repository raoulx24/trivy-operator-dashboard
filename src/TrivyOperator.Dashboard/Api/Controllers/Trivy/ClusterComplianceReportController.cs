using Microsoft.AspNetCore.Mvc;
using TrivyOperator.Dashboard.Application.Queries.Trivy.Models;
using TrivyOperator.Dashboard.Application.Queries.Trivy.Services.ClusterComplianceReports.Abstractions;

namespace TrivyOperator.Dashboard.Api.Controllers.Trivy;

[ApiController]
[Route("api/cluster-compliance-reports")]
public class ClusterComplianceReportController(IClusterComplianceReportService clusterComplianceReportService)
    : ControllerBase
{
    [HttpGet(Name = "GetClusterComplianceReportDtos")]
    [ProducesResponseType<IEnumerable<ClusterComplianceReportDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status500InternalServerError)]
    public async Task<IEnumerable<ClusterComplianceReportDto>> Get() =>
        await clusterComplianceReportService.GetClusterComplianceReportDtos();
    
    [HttpGet("{uid}", Name = "GetClusterComplianceReportDtoByUid")]
    [ProducesResponseType<ClusterComplianceReportDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetByUid(
        string uid,
        CancellationToken ctx = default)
    {
        ClusterComplianceReportDto? result =
            await clusterComplianceReportService
                .GetClusterComplianceReportDtoByUid(uid, ctx);

        return result is null ? NotFound() : Ok(result);
    }

    [HttpGet("denormalized", Name = "GetClusterComplianceReportDenormalizedDtos")]
    [ProducesResponseType<IEnumerable<ClusterComplianceReportDenormalizedDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status500InternalServerError)]
    public async Task<IEnumerable<ClusterComplianceReportDenormalizedDto>> GetDenormalized() =>
        await clusterComplianceReportService.GetClusterComplianceReportDenormalizedDtos();
}
