using Microsoft.AspNetCore.Mvc;
using TrivyOperator.Dashboard.Application.Models;
using TrivyOperator.Dashboard.Application.Services.Trivy.InfraAssessmentReport.Abstractions;
using TrivyOperator.Dashboard.Utils;

namespace TrivyOperator.Dashboard.Application.Controllers;

[ApiController]
[Route("api/infra-assessment-reports")]
public class InfraAssessmentReportController(IInfraAssessmentReportService InfraAssessmentReportService) : ControllerBase
{
    [HttpGet(Name = "GetInfraAssessmentReportDtos")]
    [ProducesResponseType<IEnumerable<InfraAssessmentReportDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status500InternalServerError)]
    public async Task<IResult> Get(string? namespaceName, string? excludedSeverities)
    {
        List<int>? excludedSeverityIds = TrivyUtils.GetExcludedSeverityIdsFromStringList(excludedSeverities);

        if (excludedSeverityIds == null)
        {
            return Results.BadRequest();
        }

        IEnumerable<InfraAssessmentReportDto> InfraAssessmentReportImageDtos =
            await InfraAssessmentReportService.GetInfraAssessmentReportDtos(namespaceName, excludedSeverityIds);

        return Results.Ok(InfraAssessmentReportImageDtos);
    }


    [HttpGet("{uid:guid}", Name = "GetInfraAssessmentReportDtoByUid")]
    [ProducesResponseType<InfraAssessmentReportDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status500InternalServerError)]
    public async Task<IResult> GetByUid(Guid uid)
    {
        InfraAssessmentReportDto? InfraAssessmentReportDto =
            await InfraAssessmentReportService.GetInfraAssessmentReportDtoByUid(uid);

        return InfraAssessmentReportDto is null
            ? Results.NotFound()
            : Results.Ok(InfraAssessmentReportDto);
    }


    [HttpGet("denormalized", Name = "GetInfraAssessmentReportDenormalizedDtos")]
    [ProducesResponseType<IEnumerable<InfraAssessmentReportDenormalizedDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status500InternalServerError)]
    public async Task<IEnumerable<InfraAssessmentReportDenormalizedDto>> GetDenormalized() =>
        await InfraAssessmentReportService.GetInfraAssessmentReportDenormalizedDtos();

    [HttpGet("active-namespaces", Name = "GetInfraAssessmentReportActiveNamespaces")]
    [ProducesResponseType<IEnumerable<string>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status500InternalServerError)]
    public async Task<IEnumerable<string>> GetActiveNamespaces() =>
        await InfraAssessmentReportService.GetActiveNamespaces();
}
