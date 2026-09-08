using Microsoft.AspNetCore.Mvc;
using TrivyOperator.Dashboard.Application.Queries.Alerts.Models;
using TrivyOperator.Dashboard.Application.Queries.Alerts.Services.Abstractions;

namespace TrivyOperator.Dashboard.Api.Alerts.Controllers;

[ApiController]
[Route("api/alerts")]
public class AlertsController(IAlertsService alertsService) : ControllerBase
{
    [HttpGet(Name = "GetAlerts")]
    [ProducesResponseType<IEnumerable<AlertDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status500InternalServerError)]
    public async Task<IEnumerable<AlertDto>> GetAll() => await alertsService.GetAlertDtos();
}
