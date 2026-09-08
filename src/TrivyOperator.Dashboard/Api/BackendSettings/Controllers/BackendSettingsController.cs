using Microsoft.AspNetCore.Mvc;
using TrivyOperator.Dashboard.Application.Queries.BackendSettings.Models;
using TrivyOperator.Dashboard.Application.Queries.BackendSettings.Services.Abstractions;

namespace TrivyOperator.Dashboard.Api.BackendSettings.Controllers;

[ApiController]
[Route("api/backend-settings")]
public class BackendSettingsController(IBackendSettingsService service) : ControllerBase
{
    [HttpGet(Name = "GetBackendSettings")]
    [ProducesResponseType<BackendSettingsDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status500InternalServerError)]
    public async Task<BackendSettingsDto> GetBackendSettings() => await service.GetBackendSettings();
}
