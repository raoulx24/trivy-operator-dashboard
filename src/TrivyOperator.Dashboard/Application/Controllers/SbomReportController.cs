﻿using Microsoft.AspNetCore.Mvc;
using System.Text;
using System.Xml.Serialization;
using TrivyOperator.Dashboard.Application.Models;
using TrivyOperator.Dashboard.Application.Services.Trivy.SbomReport.Abstractions;

namespace TrivyOperator.Dashboard.Application.Controllers;

[ApiController]
[Route("api/sbom-reports")]
public class SbomReportController(ISbomReportService sbomReportService) : ControllerBase
{
    [HttpGet(Name = "GetSbomReportDtos")]
    [ProducesResponseType<IEnumerable<SbomReportDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status500InternalServerError)]
    public async Task<IEnumerable<SbomReportDto>> Get([FromQuery] string? namespaceName) =>
        await sbomReportService.GetSbomReportDtos(namespaceName);

    [HttpGet("{uid}", Name = "GetSbomReportDtoByUid")]
    [ProducesResponseType<SbomReportDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetByUid(Guid uid)
    {
        SbomReportDto? sbomReportDto = await sbomReportService.GetFullSbomReportDtoByUid(uid.ToString());

        return sbomReportDto is null ? NotFound() : Ok(sbomReportDto);
    }

    [HttpGet("digest", Name = "GetSbomReportDtoByDigestNamespace")]
    [ProducesResponseType<SbomReportDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetByDigestNamespace([FromQuery] string digest, [FromQuery] string namespaceName)
    {
        SbomReportDto? sbomReportDto = await sbomReportService.GetFullSbomReportDtoByDigestNamespace(digest, namespaceName);

        return sbomReportDto is null ? NotFound() : Ok(sbomReportDto);
    }

    [HttpGet("cyclonedx", Name = "GetCycloneDxDtoByDigestNamespace")]
    [Produces("application/json", "application/xml")]
    [ProducesResponseType<CycloneDxBom>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetCycloneDxByDigestNamespace([FromQuery] string digest, [FromQuery] string namespaceName)
    {
        CycloneDxBom? cycloneDxBom = await sbomReportService.GetCycloneDxBomByDigestNamespace(digest, namespaceName);

        return cycloneDxBom is null ? NotFound() : Ok(cycloneDxBom);
    }

    [HttpGet("spdx", Name = "GetSpdxDtoByDigestNamespace")]
    [ProducesResponseType<SpdxBom>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetSpdxBomByDigestNamespace([FromQuery] string digest, [FromQuery] string namespaceName)
    {
        SpdxBom? spdxBom = await sbomReportService.GetSpdxBomByDigestNamespace(digest, namespaceName);

        return spdxBom is null ? NotFound() : Ok(spdxBom);
    }

    [HttpGet("grouped-by-image", Name = "GetSbomReportImageDtos")]
    [ProducesResponseType<IEnumerable<SbomReportImageDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetGroupedByImage([FromQuery] string? namespaceName) =>
        Ok(await sbomReportService.GetSbomReportImageDtos(namespaceName));

    [HttpPost("export", Name = "ExportSbomReport")]
    [Produces("application/zip")]
    [ProducesResponseType<FileStreamResult>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Export([FromBody] SbomReportExportDto[] exportSboms, [FromQuery] string fileType = "json")
    {
        if (exportSboms.Length == 0)
        {
            return BadRequest("No info provided");
        }

        string zipFilename  = await sbomReportService.CreateCycloneDxExportZipFile(exportSboms, fileType);
        if (zipFilename == string.Empty)
        {
            return BadRequest("Failed to create zip file");
        }

        var stream = new FileStream(zipFilename, FileMode.Open, FileAccess.Read);

        HttpContext.Response.OnCompleted(() =>
        {
            sbomReportService.CleanupFile(zipFilename);

            return Task.CompletedTask;
        });

        return File(stream, "application/zip", zipFilename);
    }

    [HttpGet("active-namespaces", Name = "GetSbomReportActiveNamespaces")]
    [ProducesResponseType<IEnumerable<string>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status500InternalServerError)]
    public async Task<IEnumerable<string>> GetActiveNamespaces() =>
        await sbomReportService.GetActiveNamespaces();
}
