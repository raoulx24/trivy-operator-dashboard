using Microsoft.AspNetCore.Mvc;
using TrivyOperator.Dashboard.Application.Trivy.Queries.Trivy.Models;
using TrivyOperator.Dashboard.Application.Trivy.Queries.Trivy.Services.SbomReports.Abstractions;

namespace TrivyOperator.Dashboard.Api.Trivy.Controllers;

[ApiController]
[Route("api/sbom-reports")]
public class SbomReportController(ISbomReportService sbomReportService) : ControllerBase
{
    [HttpGet("grouped-by-image/minimal", Name = "GetSbomReportImageMinimalDtos")]
    [ProducesResponseType(typeof(IEnumerable<SbomReportImageMinimalDto>), StatusCodes.Status200OK, "application/json")]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError, "application/problem+json")]
    public async Task<IActionResult> GetGroupedByImageMinimal(CancellationToken ctx) =>
        Ok(await sbomReportService.GetSbomReportImageMinimalDtos(ctx));

    [HttpGet("digest", Name = "GetSbomReportImageDtoByDigest")]
    [ProducesResponseType(typeof(SbomReportImageDto), StatusCodes.Status200OK, "application/json")]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest, "application/problem+json")]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound, "application/problem+json")]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError, "application/problem+json")]
    public async Task<IActionResult> GetByDigest([FromQuery] string digest)
    {
        SbomReportImageDto? sbomReportDto =
            await sbomReportService.GetFullSbomReportImageDtoByDigest(digest);

        return sbomReportDto is null
            ? Problem(statusCode: StatusCodes.Status404NotFound, title: "SBOM report not found")
            : Ok(sbomReportDto);
    }

    [HttpGet("cyclonedx", Name = "GetCycloneDxDtoByDigest")]
    [Produces("application/json", "application/xml")]
    [ProducesResponseType(typeof(CycloneDxBom), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest, "application/problem+json")]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound, "application/problem+json")]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError, "application/problem+json")]
    public async Task<IActionResult> GetCycloneDxByDigestNamespace(
        [FromQuery] string digest,
        CancellationToken ctx
    )
    {
        CycloneDxBom? cycloneDxBom = await sbomReportService.GetCycloneDxBomByDigest(digest, ctx);

        return cycloneDxBom is null
            ? Problem(statusCode: StatusCodes.Status404NotFound, title: "CycloneDX SBOM not found")
            : Ok(cycloneDxBom);
    }

    [HttpGet("spdx", Name = "GetSpdxDtoByDigest")]
    [ProducesResponseType(typeof(SpdxBom), StatusCodes.Status200OK, "application/json")]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest, "application/problem+json")]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound, "application/problem+json")]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError, "application/problem+json")]
    public async Task<IActionResult> GetSpdxBomByDigestNamespace(
        [FromQuery] string digest,
        CancellationToken ctx
    )
    {
        SpdxBom? spdxBom = await sbomReportService.GetSpdxBomByDigest(digest, ctx);

        return spdxBom is null
            ? Problem(statusCode: StatusCodes.Status404NotFound, title: "SPDX SBOM not found")
            : Ok(spdxBom);
    }

    [HttpPost("export", Name = "ExportSbomReport")]
    [ProducesResponseType(typeof(FileStreamResult), StatusCodes.Status200OK, "application/zip")]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest, "application/problem+json")]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError, "application/problem+json")]
    public async Task<IActionResult> Export(
        [FromBody] SbomReportExportDto[] exportSboms,
        CancellationToken ctx,
        [FromQuery] string fileType = "json")
    {
        if (exportSboms.Length == 0)
        {
            return Problem(statusCode: StatusCodes.Status400BadRequest, title: "No info provided");
        }

        SbomExportFileDto? export = await sbomReportService
            .CreateCycloneDxExportZipFile(exportSboms, fileType, ctx);

        if (export is null)
        {
            return Problem(statusCode: StatusCodes.Status400BadRequest, title: "Failed to create zip file");
        }

        return File(
            export.Stream,
            "application/zip",
            export.FileName
        );
    }

    [HttpGet("active-namespaces", Name = "GetSbomReportActiveNamespaces")]
    [ProducesResponseType(typeof(string[]), StatusCodes.Status200OK, "application/json")]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError, "application/problem+json")]
    public async Task<IActionResult> GetActiveNamespaces(CancellationToken ctx) =>
        Ok(await sbomReportService.GetActiveNamespaces(ctx));
}
