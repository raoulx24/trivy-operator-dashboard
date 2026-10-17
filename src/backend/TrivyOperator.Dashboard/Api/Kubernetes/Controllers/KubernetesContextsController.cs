using Microsoft.AspNetCore.Mvc;
using TrivyOperator.Dashboard.Application.Kubernetes.Models;
using TrivyOperator.Dashboard.Application.Kubernetes.Queries.Contexts.Abstractions;

namespace TrivyOperator.Dashboard.Api.Kubernetes.Controllers;

[ApiController]
[Route("api/kubernetes-contexts")]
public class KubernetesContextsController(IKubernetesContextService kubernetesContextService) : ControllerBase
{
    [HttpGet(Name = "GetKubernetesContexts")]
    [ProducesResponseType(typeof(KubernetesContextsDto), StatusCodes.Status200OK, "application/json")]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError, "application/problem+json")]
    public async Task<IActionResult> GetKubernetesContexts(CancellationToken ctx) =>
        Ok(await kubernetesContextService.GetKubernetesContextsDto(ctx));
}
