using Microsoft.AspNetCore.Mvc;
using TrivyOperator.Dashboard.Application.Kubernetes.Models;
using TrivyOperator.Dashboard.Application.Queries.Contexts.Abstractions;
using TrivyOperator.Dashboard.Application.Shared.Models;

namespace TrivyOperator.Dashboard.Api.Kubernetes.Controllers;

[ApiController]
[Route("api/kubernetes-contexts")]
public class KubernetesContextsController(IKubernetesContextService kubernetesContextService)
{
    [HttpGet(Name = "GetKubernetesContexts")]
    [ProducesResponseType<KubernetesContextsDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status500InternalServerError)]
    public async Task<KubernetesContextsDto> GetKubernetesContexts(CancellationToken ctx) =>
        await kubernetesContextService.GetKubernetesContextsDto(ctx);
}
