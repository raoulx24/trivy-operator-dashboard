﻿using Microsoft.AspNetCore.Mvc;
using TrivyOperator.Dashboard.Application.Models;
using TrivyOperator.Dashboard.Application.Services.Trivy.ClusterSbomReport.Abstractions;
using TrivyOperator.Dashboard.Application.Services.Trivy.VulnerabilityReport;

namespace TrivyOperator.Dashboard.Application.Controllers;

[ApiController]
[Route("api/cluster-sbom-reports")]
public class ClusterSbomReportController(IClusterSbomReportService clusterSbomReportService) : ControllerBase
{
    [HttpGet(Name = "GetClusterSbomReportDtos")]
    [ProducesResponseType<IEnumerable<ClusterSbomReportDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status500InternalServerError)]
    public async Task<IEnumerable<ClusterSbomReportDto>> Get() =>
        await clusterSbomReportService.GetClusterSbomReportDtos();

    [HttpGet("denormalized", Name = "GetClusterSbomReportDenormalizedDtos")]
    [ProducesResponseType<IEnumerable<ClusterSbomReportDenormalizedDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status500InternalServerError)]
    public async Task<IEnumerable<ClusterSbomReportDenormalizedDto>> GetDenormalized() =>
        await clusterSbomReportService.GetClusterSbomReportDenormalizedDtos();
}
