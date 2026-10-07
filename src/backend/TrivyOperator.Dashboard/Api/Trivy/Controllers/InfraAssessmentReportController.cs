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
    [ProducesResponseType<IEnumerable<InfraAssessmentReportDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Get(
        string? namespaceName,
        string? excludedSeverities,
        CancellationToken ctx = default)
    {
        QueryResponse<IEnumerable<InfraAssessmentReportDto>> result = 
            await infraAssessmentReportService.GetInfraAssessmentReportDtos(namespaceName, excludedSeverities, ctx);
        
        return result.Error is null
            ? Ok(result.Payload)
            : BadRequest(result.Error);
    }

    [HttpGet("{uid}", Name = "GetInfraAssessmentReportDtoByUid")]
    [ProducesResponseType<InfraAssessmentReportDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<InfraAssessmentReportDto>> GetByUid(
        string uid,
        CancellationToken ctx = default)
    {
        InfraAssessmentReportDto? result =
            await infraAssessmentReportService.GetInfraAssessmentReportDtoByUid(uid, ctx);

        return result is null
            ? NotFound()
            : Ok(result);
    }

    [HttpGet("denormalized", Name = "GetInfraAssessmentReportDenormalizedDtos")]
    [ProducesResponseType<IEnumerable<InfraAssessmentReportDenormalizedDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status500InternalServerError)]
    public async Task<IEnumerable<InfraAssessmentReportDenormalizedDto>> GetDenormalized(
        string? namespaceName,
        CancellationToken ctx = default) =>
        await infraAssessmentReportService
            .GetInfraAssessmentReportDenormalizedDtos(namespaceName, ctx);
    
    [HttpGet("active-namespaces", Name = "GetInfraAssessmentReportActiveNamespaces")]
    [ProducesResponseType<string[]>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetActiveNamespaces(CancellationToken ctx)
    {
        return Ok(await infraAssessmentReportService.GetActiveNamespaces(ctx));
    }
}
