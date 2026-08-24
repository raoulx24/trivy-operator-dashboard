using Microsoft.AspNetCore.Mvc;
using TrivyOperator.Dashboard.Application.Queries.Trivy.Models;
using TrivyOperator.Dashboard.Application.Queries.Trivy.Services.SbomReports.Abstractions;

namespace TrivyOperator.Dashboard.Api.Trivy.Controllers;

[ApiController]
[Route("api/sbom-reports")]
public class SbomReportController(ISbomReportService sbomReportService) : ControllerBase
{
    [HttpGet("grouped-by-image/minimal", Name = "GetSbomReportImageMinimalDtos")]
    [ProducesResponseType<IEnumerable<SbomReportImageMinimalDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetGroupedByImageMinimal(CancellationToken ctx) =>
        Ok(await sbomReportService.GetSbomReportImageMinimalDtos(ctx));
    

    [HttpGet("digest", Name = "GetSbomReportImageDtoByDigest")]
    [ProducesResponseType<SbomReportImageDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetByDigest([FromQuery] string digest)
    {
        SbomReportImageDto? sbomReportDto =
            await sbomReportService.GetFullSbomReportImageDtoByDigest(digest);

        return sbomReportDto is null ? NotFound() : Ok(sbomReportDto);
    }


    [HttpGet("cyclonedx", Name = "GetCycloneDxDtoByDigest")]
    [Produces("application/json", "application/xml")]
    [ProducesResponseType<CycloneDxBom>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetCycloneDxByDigestNamespace(
        [FromQuery] string digest,
        CancellationToken ctx
    )
    {
        CycloneDxBom? cycloneDxBom = await sbomReportService.GetCycloneDxBomByDigest(digest, ctx);

        return cycloneDxBom is null ? NotFound() : Ok(cycloneDxBom);
    }

    [HttpGet("spdx", Name = "GetSpdxDtoByDigest")]
    [ProducesResponseType<SpdxBom>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetSpdxBomByDigestNamespace(
        [FromQuery] string digest,
        CancellationToken ctx
    )
    {
        SpdxBom? spdxBom = await sbomReportService.GetSpdxBomByDigest(digest, ctx);

        return spdxBom is null ? NotFound() : Ok(spdxBom);
    }

    
    [HttpPost("export", Name = "ExportSbomReport")]
    [Produces("application/zip")]
    [ProducesResponseType<FileStreamResult>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Export(
        [FromBody] SbomReportExportDto[] exportSboms,
        CancellationToken ctx,
        [FromQuery] string fileType = "json")
    {
        if (exportSboms.Length == 0)
        {
            return BadRequest("No info provided");
        }

        SbomExportFileDto? export = await sbomReportService
            .CreateCycloneDxExportZipFile(exportSboms, fileType, ctx);

        if (export is null)
        {
            return BadRequest("Failed to create zip file");
        }

        return File(
            export.Stream,
            "application/zip",
            export.FileName
        );
    }
}
