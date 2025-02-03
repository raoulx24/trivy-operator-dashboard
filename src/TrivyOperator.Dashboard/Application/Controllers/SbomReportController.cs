﻿using Microsoft.AspNetCore.Mvc;
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
        SbomReportDto? sbomReportDto = await sbomReportService.GetSbomReportDtoByUid(uid);

        return sbomReportDto is null ? NotFound() : Ok(sbomReportDto);
    }

    //[HttpGet("", Name = "GetSbomReportDtoByUidNameNamespace")]
    //[ProducesResponseType<SbomReportDto>(StatusCodes.Status200OK)]
    //[ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    //[ProducesResponseType<ProblemDetails>(StatusCodes.Status500InternalServerError)]
    //public async Task<IActionResult> GetByUidNameNamespace(
    //    [FromQuery] Guid uid,
    //    [FromQuery] string resourceName,
    //    [FromQuery] string namespaceName)
    //{
    //    SbomReportDto? sbomReportDto = await sbomReportService.GetSbomReportDtoByUidNamespace(uid, namespaceName);

    //    if (sbomReportDto is null)
    //    {
    //        sbomReportDto = await sbomReportService.GetSbomReportDtoByResourceName(resourceName, namespaceName);
    //    }

    //    return sbomReportDto is null ? NotFound() : Ok(sbomReportDto);
    //}

    [HttpGet("active-namespaces", Name = "GetSbomReportActiveNamespaces")]
    [ProducesResponseType<IEnumerable<string>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status500InternalServerError)]
    public async Task<IEnumerable<string>> GetActiveNamespaces() =>
        await sbomReportService.GetActiveNamespaces();
}
