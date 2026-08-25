using Microsoft.AspNetCore.Mvc;
using TrivyOperator.Dashboard.Application.Queries.Trivy.Models;
using TrivyOperator.Dashboard.Application.Queries.Trivy.Services.InfraAssessmentReports.Abstractions;
using TrivyOperator.Dashboard.Application.Trivy.Utils;

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
    public async Task<IEnumerable<InfraAssessmentReportDto>> Get(
        string? namespaceName,
        string? excludedSeverities,
        CancellationToken ctx = default)
    {
        IReadOnlySet<int>? excludedSeverityIds =
            TrivyUtils.GetSeverityIdsToInclude(excludedSeverities);

        if (excludedSeverityIds is null)
            throw new BadHttpRequestException("Invalid excluded severities.");

        return await infraAssessmentReportService.GetInfraAssessmentReportDtos(
            namespaceName,
            excludedSeverityIds,
            ctx);
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
}
