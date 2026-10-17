using Microsoft.AspNetCore.Mvc;
using TrivyOperator.Dashboard.Application.Kubernetes.Queries.WatcherStates.Models;
using TrivyOperator.Dashboard.Application.Kubernetes.Queries.WatcherStates.Services.Abstractions;
using TrivyOperator.Dashboard.Application.Shared.Queries.Models;

namespace TrivyOperator.Dashboard.Api.Kubernetes.Controllers;

[ApiController]
[Route("api/watcher-status")]
public class WatcherStatusController(IWatcherStatusService watcherStateInfoService) : ControllerBase
{
    [HttpGet(Name = "GetWatcherStateInfos")]
    [ProducesResponseType(typeof(IEnumerable<WatcherStatusDto>), StatusCodes.Status200OK, "application/json")]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError, "application/problem+json")]
    public async Task<IActionResult> GetAll() => Ok(await watcherStateInfoService.GetWatcherStatusDtos());

    [HttpPost("recreate")]
    [ProducesResponseType(typeof(RecreateWatcherResponse), StatusCodes.Status200OK, "application/json")]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest, "application/problem+json")]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity, "application/problem+json")]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError, "application/problem+json")]
    public async Task<IActionResult> RecreateWatcher([FromBody] RecreateWatcherRequest request)
    {
        if (string.IsNullOrWhiteSpace(request?.KubernetesObjectType))
        {
            return Problem(
                statusCode: StatusCodes.Status400BadRequest,
                title: "Invalid request",
                detail: "KubernetesObjectType is required.");
        }

        OperationResult result = await watcherStateInfoService.RecreateWatcher(
            request.KubernetesObjectType,
            request.ContextName,
            request.NamespaceName
        );

        if (result.Success)
        {
            return Ok(
                new RecreateWatcherResponse
                {
                    Message =
                        $"Watcher for '{request.KubernetesObjectType}' in namespace '{request.NamespaceName}' recreated successfully.",
                }
            );
        }

        return Problem(
            statusCode: StatusCodes.Status422UnprocessableEntity,
            title: "Failed to recreate watcher",
            detail: result.Message);
    }
}
