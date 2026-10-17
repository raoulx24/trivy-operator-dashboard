using Microsoft.AspNetCore.Mvc;
using TrivyOperator.Dashboard.Application.Alerts.Queries.Models;
using TrivyOperator.Dashboard.Application.Alerts.Queries.Services.Abstractions;

namespace TrivyOperator.Dashboard.Api.Alerts.Controllers;

[ApiController]
[Route("api/alerts")]
public class AlertsController(IAlertsService alertsService) : ControllerBase
{
    [HttpGet(Name = "GetAlerts")]
    [ProducesResponseType(typeof(IEnumerable<AlertDto>), StatusCodes.Status200OK, "application/json")]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError, "application/problem+json")]
    public async Task<IActionResult> GetAll() => Ok(await alertsService.GetAlertDtos());
}
