using Microsoft.AspNetCore.Mvc;
using TrivyOperator.Dashboard.Application.Shared.Queries;
using TrivyOperator.Dashboard.Application.Trivy.Queries.Trivy.Models;
using TrivyOperator.Dashboard.Application.Trivy.Queries.Trivy.Services.ExposedSecretReports.Abstractions;

namespace TrivyOperator.Dashboard.Api.Trivy.Controllers;

[ApiController]
[Route("api/exposed-secret-reports")]
public class ExposedSecretReportsController(
    IExposedSecretReportService exposedSecretReportService)
    : ControllerBase
{
    [HttpGet(Name = "GetExposedSecretReportDtos")]
    [ProducesResponseType(typeof(IEnumerable<ExposedSecretReportDto>), StatusCodes.Status200OK, "application/json")]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError, "application/problem+json")]
    public async Task<IActionResult> Get(
        [FromQuery] string? namespaceName) =>
        Ok(await exposedSecretReportService.GetExposedSecretReportDtos(
            namespaceName,
            HttpContext.RequestAborted));

    [HttpGet("{uid}", Name = "GetExposedSecretReportDtoByUid")]
    [ProducesResponseType(typeof(ExposedSecretReportDto), StatusCodes.Status200OK, "application/json")]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest, "application/problem+json")]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound, "application/problem+json")]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError, "application/problem+json")]
    public async Task<IActionResult> GetByUid(
        string? uid)
    {
        if (string.IsNullOrEmpty(uid))
        {
            return Problem(
                statusCode: StatusCodes.Status400BadRequest,
                title: "Invalid request",
                detail: "A uid must be provided");
        }

        ExposedSecretReportDto? exposedSecretReportDto =
            await exposedSecretReportService.GetExposedSecretReportDtoByUid(
                uid,
                HttpContext.RequestAborted);

        return exposedSecretReportDto is null
            ? Problem(statusCode: StatusCodes.Status404NotFound, title: "Exposed secret report not found")
            : Ok(exposedSecretReportDto);
    }

    [HttpGet("denormalized", Name = "GetExposedSecretReportDenormalizedDtos")]
    [ProducesResponseType(typeof(IEnumerable<ExposedSecretReportDenormalizedDto>), StatusCodes.Status200OK, "application/json")]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError, "application/problem+json")]
    public async Task<IActionResult> GetDenormalized(
        [FromQuery] string? namespaceName) =>
        Ok(await exposedSecretReportService.GetExposedSecretReportDenormalizedDtos(
            namespaceName,
            HttpContext.RequestAborted));

    [HttpGet("grouped-by-image", Name = "GetExposedSecretReportImageDtos")]
    [ProducesResponseType(typeof(IEnumerable<ExposedSecretReportImageDto>), StatusCodes.Status200OK, "application/json")]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest, "application/problem+json")]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError, "application/problem+json")]
    public async Task<IActionResult> GetGroupedByImage(
        string? namespaceName,
        string? excludedSeverities,
        CancellationToken ctx)
    {
        QueryResponse<IEnumerable<ExposedSecretReportImageDto>> result =
            await exposedSecretReportService.GetExposedSecretReportImageDtos(namespaceName, excludedSeverities, ctx);

        return result.Error is null
            ? Ok(result.Payload)
            : Problem(statusCode: StatusCodes.Status400BadRequest, title: "Invalid request", detail: result.Error);
    }

    [HttpGet("active-namespaces", Name = "GetExposedSecretReportActiveNamespaces")]
    [ProducesResponseType(typeof(string[]), StatusCodes.Status200OK, "application/json")]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError, "application/problem+json")]
    public async Task<IActionResult> GetActiveNamespaces(CancellationToken ctx) =>
        Ok(await exposedSecretReportService.GetActiveNamespaces(ctx));
}
