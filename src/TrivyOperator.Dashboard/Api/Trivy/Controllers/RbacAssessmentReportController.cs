using Microsoft.AspNetCore.Mvc;
using TrivyOperator.Dashboard.Application.Queries.Trivy.Models;
using TrivyOperator.Dashboard.Application.Queries.Trivy.Services.RbacAssessmentReports.Abstractions;
using TrivyOperator.Dashboard.Application.Trivy.Utils;

namespace TrivyOperator.Dashboard.Api.Trivy.Controllers;

[ApiController]
[Route("api/rbac-assessment-reports")]
public class RbacAssessmentReportController(
    IRbacAssessmentReportService rbacAssessmentReportService
) : ControllerBase
{
    [HttpGet(Name = "GetRbacAssessmentReportDtos")]
    [ProducesResponseType<IEnumerable<RbacAssessmentReportDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Get(
        [FromQuery] string? namespaceName,
        [FromQuery] string? excludedSeverities,
        CancellationToken ctx = default)
    {
        IReadOnlySet<int>? includedSeverityIds =
            TrivyUtils.GetSeverityIdsToInclude(excludedSeverities);

        if (includedSeverityIds == null)
        {
            return BadRequest();
        }

        IEnumerable<RbacAssessmentReportDto> result =
            await rbacAssessmentReportService.GetRbacAssessmentReportDtos(
                namespaceName,
                includedSeverityIds,
                ctx);

        return Ok(result);
    }

    [HttpGet("denormalized", Name = "GetRbacAssessmentReportDenormalizedDtos")]
    [ProducesResponseType<IEnumerable<RbacAssessmentReportDenormalizedDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status500InternalServerError)]
    public async Task<IEnumerable<RbacAssessmentReportDenormalizedDto>> GetDenormalized(
        [FromQuery] string? namespaceName,
        CancellationToken ctx = default) =>
        await rbacAssessmentReportService
            .GetRbacAssessmentReportDenormalizedDtos(namespaceName, ctx);
}
