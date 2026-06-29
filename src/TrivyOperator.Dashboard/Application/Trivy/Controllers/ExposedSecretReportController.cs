using Microsoft.AspNetCore.Mvc;
using TrivyOperator.Dashboard.Application.Trivy.Models;
using TrivyOperator.Dashboard.Application.Trivy.Services.ExposedSecretReport.Abstractions;
using TrivyOperator.Dashboard.Domain.Trivy;
using TrivyOperator.Dashboard.Domain.TrivyOld;

namespace TrivyOperator.Dashboard.Application.Trivy.Controllers;

[ApiController]
[Route("api/exposed-secret-reports")]
public class ExposedSecretReportController(IExposedSecretReportService exposedSecretReportService) : ControllerBase
{
    [HttpGet(Name = "GetExposedSecretReportDtos")]
    [ProducesResponseType<IEnumerable<ExposedSecretReportDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status500InternalServerError)]
    public async Task<IResult> Get([FromQuery] string? namespaceName, [FromQuery] string? excludedSeverities)
    {
        List<int>? excludedSeverityIds = TrivyUtils.GetExcludedSeverityIdsFromStringList(excludedSeverities);

        if (excludedSeverityIds == null)
        {
            return Results.BadRequest();
        }

        IEnumerable<ExposedSecretReportDto> exposedSecretReportImageDtos =
            await exposedSecretReportService.GetExposedSecretReportDtos(namespaceName, excludedSeverityIds);
        return Results.Ok(exposedSecretReportImageDtos);
    }

    [HttpGet("denormalized", Name = "GetExposedSecretReportDenormalizedDtos")]
    [ProducesResponseType<IEnumerable<ExposedSecretReportDenormalizedDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status500InternalServerError)]
    public async Task<IEnumerable<ExposedSecretReportDenormalizedDto>> GetDenormalized() =>
        await exposedSecretReportService.GetExposedSecretDenormalizedDtos();

    [HttpGet("active-namespaces", Name = "GetExposedSecretReportActiveNamespaces")]
    [ProducesResponseType<IEnumerable<string>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status500InternalServerError)]
    public async Task<IEnumerable<string>> GetActiveNamespaces() =>
        await exposedSecretReportService.GetActiveNamespaces();

    [HttpGet("grouped-by-image", Name = "GetExposedSecretReportImageDtos")]
    [ProducesResponseType<IEnumerable<ExposedSecretReportImageDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status500InternalServerError)]
    public async Task<IResult> GetGroupedByImage(string? namespaceName, string? excludedSeverities, string? digest)
    {
        List<int>? excludedSeverityIds = TrivyUtils.GetExcludedSeverityIdsFromStringList(excludedSeverities);

        if (excludedSeverityIds == null)
        {
            return Results.BadRequest();
        }

        if (digest != null && namespaceName == null)
        {
            return Results.BadRequest("Namespace name is required when digest is provided.");
        }

        if (digest != null && namespaceName != null)
        {
            ExposedSecretReportImageDto? exposedSecretReportImageDto =
                await exposedSecretReportService.GetExposedSecretReportImageDtoByDigestNamespace(digest, namespaceName);

            return exposedSecretReportImageDto is null ? Results.NotFound() : Results.Ok(
                new[]
                {
                    exposedSecretReportImageDto,
                }
            );
        }

        IEnumerable<ExposedSecretReportImageDto> exposedSecretReportImageDtos =
            await exposedSecretReportService.GetExposedSecretReportImageDtos(namespaceName, excludedSeverityIds);

        return Results.Ok(exposedSecretReportImageDtos);
    }

    [HttpGet("summary", Name = "GetExposedSecretReportSummaryDtos")]
    [ProducesResponseType<IEnumerable<EsSeveritiesByNsSummaryDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status500InternalServerError)]
    public async Task<IEnumerable<EsSeveritiesByNsSummaryDto>> GetExposedSecretReportSummaryDtos() =>
        await exposedSecretReportService.GetExposedSecretReportSummaryDtos();
}
