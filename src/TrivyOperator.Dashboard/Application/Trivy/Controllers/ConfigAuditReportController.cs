using Microsoft.AspNetCore.Mvc;
using TrivyOperator.Dashboard.Application.Trivy.Models;
using TrivyOperator.Dashboard.Application.Trivy.Services.ConfigAuditReport.Abstractions;
using TrivyOperator.Dashboard.Domain.Trivy;

namespace TrivyOperator.Dashboard.Application.Trivy.Controllers;

[ApiController]
[Route("api/config-audit-reports")]
public class ConfigAuditReportController(IConfigAuditReportService configAuditReportService) : ControllerBase
{
    [HttpGet(Name = "GetConfigAuditReportDtos")]
    [ProducesResponseType<IEnumerable<ConfigAuditReportDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status500InternalServerError)]
    public async Task<IResult> Get(string? namespaceName, string? excludedSeverities)
    {
        List<int>? excludedSeverityIds = TrivyUtils.GetExcludedSeverityIdsFromStringList(excludedSeverities);

        if (excludedSeverityIds == null)
        {
            return Results.BadRequest();
        }

        IEnumerable<ConfigAuditReportDto> configAuditReportImageDtos =
            await configAuditReportService.GetConfigAuditReportDtos(namespaceName, excludedSeverityIds);

        return Results.Ok(configAuditReportImageDtos);
    }


    [HttpGet("{uid:guid}", Name = "GetConfigAuditReportDtoByUid")]
    [ProducesResponseType<ConfigAuditReportDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status500InternalServerError)]
    public async Task<IResult> GetByUid(Guid uid)
    {
        ConfigAuditReportDto? configAuditReportDto =
            await configAuditReportService.GetConfigAuditReportDtoByUid(uid);

        return configAuditReportDto is null
            ? Results.NotFound()
            : Results.Ok(configAuditReportDto);
    }



    [HttpGet("denormalized", Name = "GetConfigAuditReportDenormalizedDtos")]
    [ProducesResponseType<IEnumerable<ConfigAuditReportDenormalizedDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status500InternalServerError)]
    public async Task<IEnumerable<ConfigAuditReportDenormalizedDto>> GetDenormalized() =>
        await configAuditReportService.GetConfigAuditReportDenormalizedDtos();

    [HttpGet("active-namespaces", Name = "GetConfigAuditReportActiveNamespaces")]
    [ProducesResponseType<IEnumerable<string>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status500InternalServerError)]
    public async Task<IEnumerable<string>> GetActiveNamespaces() =>
        await configAuditReportService.GetActiveNamespaces();

    [HttpGet("summary", Name = "GetConfigAuditReportSummaryDtos")]
    [ProducesResponseType<IEnumerable<ConfigAuditReportSummaryDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status500InternalServerError)]
    public async Task<IEnumerable<ConfigAuditReportSummaryDto>> GetConfigAuditReportSummaryDtos() =>
        await configAuditReportService.GetConfigAuditReportSummaryDtos();
}
