﻿using Microsoft.AspNetCore.Mvc;
using TrivyOperator.Dashboard.Application.Models;
using TrivyOperator.Dashboard.Application.Services.WatcherStates.Abstractions;

namespace TrivyOperator.Dashboard.Application.Controllers;

[ApiController]
[Route("api/watcher-state-infos")]
public class WatcherStateInfoController(IWatcherStateInfoService watcherStateInfoService) : ControllerBase
{
    [HttpGet(Name = "GetWatcherStateInfos")]
    [ProducesResponseType<IEnumerable<WatcherStateInfoDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status500InternalServerError)]
    public async Task<IEnumerable<WatcherStateInfoDto>> GetAll() =>
        await watcherStateInfoService.GetWatcherStateInfos();
}
