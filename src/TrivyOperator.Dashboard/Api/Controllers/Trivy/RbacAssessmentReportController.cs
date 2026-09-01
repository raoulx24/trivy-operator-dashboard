using Microsoft.AspNetCore.Mvc;
using TrivyOperator.Dashboard.Application.Queries.Shared;
using TrivyOperator.Dashboard.Application.Queries.Trivy.Models;
using TrivyOperator.Dashboard.Application.Queries.Trivy.Services.RbacAssessmentReports.Abstractions;

namespace TrivyOperator.Dashboard.Api.Controllers.Trivy;

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
        QueryResponse<IEnumerable<RbacAssessmentReportDto>> result =
            await rbacAssessmentReportService.GetRbacAssessmentReportDtos(
                namespaceName,
                excludedSeverities,
                ctx);

        return result.Error is null
            ? Ok(result.Payload)
            : BadRequest(result.Error);
    }
    
    [HttpGet("{uid}", Name = "GetRbacAssessmentReportDtoByUid")]
    [ProducesResponseType<RbacAssessmentReportDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetByUid(
        string uid,
        CancellationToken ctx = default)
    {
        RbacAssessmentReportDto? result =
            await rbacAssessmentReportService
                .GetRbacAssessmentReportDtoByUid(uid, ctx);

        return result is null ? NotFound() : Ok(result);
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
