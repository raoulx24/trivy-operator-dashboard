using Microsoft.AspNetCore.Mvc;
using TrivyOperator.Dashboard.Application.Shared.Queries;
using TrivyOperator.Dashboard.Application.Trivy.Queries.Trivy.Models;
using TrivyOperator.Dashboard.Application.Trivy.Queries.Trivy.Services.InfraAssessmentReports.Abstractions;

namespace TrivyOperator.Dashboard.Api.Trivy.Controllers;

[ApiController]
[Route("api/infra-assessment-reports")]
public class InfraAssessmentReportController(
    IInfraAssessmentReportService infraAssessmentReportService
) : ControllerBase
{
    [HttpGet(Name = "GetInfraAssessmentReportDtos")]
    [ProducesResponseType(typeof(IEnumerable<InfraAssessmentReportDto>), StatusCodes.Status200OK, "application/json")]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest, "application/problem+json")]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError, "application/problem+json")]
    public async Task<IActionResult> Get(
        string? namespaceName,
        string? excludedSeverities,
        CancellationToken ctx = default)
    {
        QueryResponse<IEnumerable<InfraAssessmentReportDto>> result =
            await infraAssessmentReportService.GetInfraAssessmentReportDtos(namespaceName, excludedSeverities, ctx);

        return result.Error is null
            ? Ok(result.Payload)
            : Problem(statusCode: StatusCodes.Status400BadRequest, title: "Invalid request", detail: result.Error);
    }

    [HttpGet("{uid}", Name = "GetInfraAssessmentReportDtoByUid")]
    [ProducesResponseType(typeof(InfraAssessmentReportDto), StatusCodes.Status200OK, "application/json")]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest, "application/problem+json")]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound, "application/problem+json")]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError, "application/problem+json")]
    public async Task<IActionResult> GetByUid(
        string uid,
        CancellationToken ctx = default)
    {
        InfraAssessmentReportDto? result =
            await infraAssessmentReportService.GetInfraAssessmentReportDtoByUid(uid, ctx);

        return result is null
            ? Problem(statusCode: StatusCodes.Status404NotFound, title: "Infra assessment report not found")
            : Ok(result);
    }

    [HttpGet("denormalized", Name = "GetInfraAssessmentReportDenormalizedDtos")]
    [ProducesResponseType(typeof(IEnumerable<InfraAssessmentReportDenormalizedDto>), StatusCodes.Status200OK, "application/json")]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError, "application/problem+json")]
    public async Task<IActionResult> GetDenormalized(
        string? namespaceName,
        CancellationToken ctx = default) =>
        Ok(await infraAssessmentReportService
            .GetInfraAssessmentReportDenormalizedDtos(namespaceName, ctx));

    [HttpGet("active-namespaces", Name = "GetInfraAssessmentReportActiveNamespaces")]
    [ProducesResponseType(typeof(string[]), StatusCodes.Status200OK, "application/json")]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError, "application/problem+json")]
    public async Task<IActionResult> GetActiveNamespaces(CancellationToken ctx) =>
        Ok(await infraAssessmentReportService.GetActiveNamespaces(ctx));
}
