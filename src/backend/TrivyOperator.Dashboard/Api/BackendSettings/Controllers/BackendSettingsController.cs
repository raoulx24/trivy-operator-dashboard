using Microsoft.AspNetCore.Mvc;
using TrivyOperator.Dashboard.Application.BackendSettings.Queries.Models;
using TrivyOperator.Dashboard.Application.BackendSettings.Queries.Services.Abstractions;

namespace TrivyOperator.Dashboard.Api.BackendSettings.Controllers;

[ApiController]
[Route("api/backend-settings")]
public class BackendSettingsController(IBackendSettingsService service) : ControllerBase
{
    [HttpGet(Name = "GetBackendSettings")]
    [ProducesResponseType(typeof(BackendSettingsDto), StatusCodes.Status200OK, "application/json")]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError, "application/problem+json")]
    public async Task<IActionResult> GetBackendSettings() => Ok(await service.GetBackendSettings());
}
