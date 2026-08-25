using Microsoft.AspNetCore.Mvc;
using TrivyOperator.Dashboard.Application.Queries.Trivy.Models;
using TrivyOperator.Dashboard.Application.Queries.Trivy.Services.ExposedSecretReports.Abstractions;
using TrivyOperator.Dashboard.Application.Trivy.Utils;

namespace TrivyOperator.Dashboard.Application.Trivy.Controllers;

[ApiController]
[Route("api/exposed-secret-reports")]
public class ExposedSecretReportsController(
    IExposedSecretReportService exposedSecretReportService)
    : ControllerBase
{
    [HttpGet(Name = "GetExposedSecretReportDtos")]
    [ProducesResponseType<IEnumerable<ExposedSecretReportDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status500InternalServerError)]
    public async Task<IEnumerable<ExposedSecretReportDto>> Get(
        [FromQuery] string? namespaceName)
        => await exposedSecretReportService.GetExposedSecretReportDtos(
            namespaceName,
            HttpContext.RequestAborted);

    [HttpGet("{uid}", Name = "GetExposedSecretReportDtoByUid")]
    [ProducesResponseType<ExposedSecretReportDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status500InternalServerError)]
    public async Task<IResult> GetByUid(
        string? uid)
    {
        if (string.IsNullOrEmpty(uid))
            return Results.BadRequest("A uid must be provided");

        ExposedSecretReportDto? exposedSecretReportDto =
            await exposedSecretReportService.GetExposedSecretReportDtoByUid(
                uid,
                HttpContext.RequestAborted);

        return exposedSecretReportDto is null
            ? Results.NotFound()
            : Results.Ok(exposedSecretReportDto);
    }

    [HttpGet("denormalized", Name = "GetExposedSecretReportDenormalizedDtos")]
    [ProducesResponseType<IEnumerable<ExposedSecretReportDenormalizedDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status500InternalServerError)]
    public async Task<IEnumerable<ExposedSecretReportDenormalizedDto>> GetDenormalized(
        [FromQuery] string? namespaceName)
        => await exposedSecretReportService.GetExposedSecretReportDenormalizedDtos(
            namespaceName,
            HttpContext.RequestAborted);

    [HttpGet("grouped-by-image", Name = "GetExposedSecretReportImageDtos")]
    [ProducesResponseType<IEnumerable<ExposedSecretReportImageDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status500InternalServerError)]
    public async Task<IResult> GetGroupedByImage(
        string? namespaceName,
        string? excludedSeverities,
        CancellationToken ctx)
    {
        IReadOnlySet<int>? includedSeverityIds =
            TrivyUtils.GetSeverityIdsToInclude(excludedSeverities);

        if (includedSeverityIds == null)
            return Results.BadRequest();

        if (includedSeverityIds.Count == TrivyUtils.GetAllSeverityIds().Count)
            includedSeverityIds = null;

        IEnumerable<ExposedSecretReportImageDto> exposedSecretReportImageDtos =
            await exposedSecretReportService.GetExposedSecretReportImageDtos(
                namespaceName,
                includedSeverityIds,
                ctx);

        return Results.Ok(exposedSecretReportImageDtos);
    }
}
