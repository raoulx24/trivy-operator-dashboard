﻿using Microsoft.AspNetCore.Mvc;
using TrivyOperator.Dashboard.Application.Models;
using TrivyOperator.Dashboard.Application.Services.WatcherStates.Abstractions;

namespace TrivyOperator.Dashboard.Application.Controllers;

[ApiController]
[Route("api/watcher-status")]
public class WatcherStatusController(IWatcherStatusService watcherStateInfoService) : ControllerBase
{
    [HttpGet(Name = "GetWatcherStateInfos")]
    [ProducesResponseType<IEnumerable<WatcherStatusDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status500InternalServerError)]
    public async Task<IEnumerable<WatcherStatusDto>> GetAll() =>
        await watcherStateInfoService.GetWatcherStatusDtos();

    [HttpPost("recreate")]
    [ProducesResponseType(typeof(RecreateWatcherResponse), StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status500InternalServerError)]
    public async Task<IResult> RecreateWatcher([FromBody] RecreateWatcherRequest request)
    {
        if (string.IsNullOrWhiteSpace(request?.KubernetesObjectType))
        {
            return Results.BadRequest(new RecreateWatcherResponse
            {
                Message = "Error occured",
                Error = "KubernetesObjectType is required.",
                KubernetesObjectType = request?.KubernetesObjectType,
                NamespaceName = request?.NamespaceName
            });
        }
        var result = await watcherStateInfoService.RecreateWatcher(request.KubernetesObjectType, request.NamespaceName);
        
        if (result.Success)
        {
            return Results.Ok(new RecreateWatcherResponse
            {
                Message = $"Watcher for '{request.KubernetesObjectType}' in namespace '{request.NamespaceName}' recreated successfully."
            });
        }

        return Results.UnprocessableEntity(new RecreateWatcherResponse
        {
            Message = "Error occured",
            Error = result.Message,
            KubernetesObjectType = request.KubernetesObjectType,
            NamespaceName = request.NamespaceName
        });
    }
}
