using Microsoft.AspNetCore.Mvc;
using TrivyOperator.Dashboard.Application.Shared.Queries;
using TrivyOperator.Dashboard.Application.Trivy.Queries.Trivy.Models;
using TrivyOperator.Dashboard.Application.Trivy.Queries.Trivy.Services.RbacAssessmentReports.Abstractions;

namespace TrivyOperator.Dashboard.Api.Trivy.Controllers;

[ApiController]
[Route("api/rbac-assessment-reports")]
public class RbacAssessmentReportController(
    IRbacAssessmentReportService rbacAssessmentReportService
) : ControllerBase
{
    [HttpGet(Name = "GetRbacAssessmentReportDtos")]
    [ProducesResponseType(typeof(IEnumerable<RbacAssessmentReportDto>), StatusCodes.Status200OK, "application/json")]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest, "application/problem+json")]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError, "application/problem+json")]
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
            : Problem(statusCode: StatusCodes.Status400BadRequest, title: "Invalid request", detail: result.Error);
    }

    [HttpGet("{uid}", Name = "GetRbacAssessmentReportDtoByUid")]
    [ProducesResponseType(typeof(RbacAssessmentReportDto), StatusCodes.Status200OK, "application/json")]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest, "application/problem+json")]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound, "application/problem+json")]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError, "application/problem+json")]
    public async Task<IActionResult> GetByUid(
        string uid,
        CancellationToken ctx = default)
    {
        RbacAssessmentReportDto? result =
            await rbacAssessmentReportService
                .GetRbacAssessmentReportDtoByUid(uid, ctx);

        return result is null
            ? Problem(statusCode: StatusCodes.Status404NotFound, title: "RBAC assessment report not found")
            : Ok(result);
    }

    [HttpGet("denormalized", Name = "GetRbacAssessmentReportDenormalizedDtos")]
    [ProducesResponseType(typeof(IEnumerable<RbacAssessmentReportDenormalizedDto>), StatusCodes.Status200OK, "application/json")]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError, "application/problem+json")]
    public async Task<IActionResult> GetDenormalized(
        [FromQuery] string? namespaceName,
        CancellationToken ctx = default) =>
        Ok(await rbacAssessmentReportService
            .GetRbacAssessmentReportDenormalizedDtos(namespaceName, ctx));

    [HttpGet("active-namespaces", Name = "GetRbacAssessmentReportActiveNamespaces")]
    [ProducesResponseType(typeof(string[]), StatusCodes.Status200OK, "application/json")]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError, "application/problem+json")]
    public async Task<IActionResult> GetActiveNamespaces(CancellationToken ctx) =>
        Ok(await rbacAssessmentReportService.GetActiveNamespaces(ctx));
}
