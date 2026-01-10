using Microsoft.AspNetCore.Mvc;
using TrivyOperator.Dashboard.Application.Trivy.Models;
using TrivyOperator.Dashboard.Application.Trivy.Services.RbacAssessmentReport.Abstractions;
using TrivyOperator.Dashboard.Domain.Trivy;

namespace TrivyOperator.Dashboard.Application.Trivy.Controllers;

[ApiController]
[Route("api/rbac-assessment-reports")]
public class RbacAssessmentReportController(IRbacAssessmentReportService rbacAssessmentReportService) : ControllerBase
{
    [HttpGet(Name = "GetRbacAssessmentReportDtos")]
    [ProducesResponseType<IEnumerable<RbacAssessmentReportDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Get([FromQuery] string? namespaceName, [FromQuery] string? excludedSeverities)
    {
        List<int>? excludedSeverityIds = TrivyUtils.GetExcludedSeverityIdsFromStringList(excludedSeverities);

        if (excludedSeverityIds == null)
        {
            return BadRequest();
        }

        IEnumerable<RbacAssessmentReportDto> rbacAssessmentReportImageDtos =
            await rbacAssessmentReportService.GetRbacAssessmentReportDtos(namespaceName, excludedSeverityIds);
        return Ok(rbacAssessmentReportImageDtos);
    }

    [HttpGet("denormalized", Name = "GetRbacAssessmentReportDenormalizedDtos")]
    [ProducesResponseType<IEnumerable<RbacAssessmentReportDenormalizedDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status500InternalServerError)]
    public async Task<IEnumerable<RbacAssessmentReportDenormalizedDto>> GetDenormalized() =>
        await rbacAssessmentReportService.GetRbacAssessmentReportDenormalizedDtos();

    [HttpGet("active-namespaces", Name = "GetRbacAssessmentReportActiveNamespaces")]
    [ProducesResponseType<IEnumerable<string>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status500InternalServerError)]
    public async Task<IEnumerable<string>> GetActiveNamespaces() =>
        await rbacAssessmentReportService.GetActiveNamespaces();
}
