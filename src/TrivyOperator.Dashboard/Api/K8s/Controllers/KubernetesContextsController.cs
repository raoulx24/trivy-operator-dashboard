using Microsoft.AspNetCore.Mvc;
using TrivyOperator.Dashboard.Application.K8sEventPipeline.Models;
using TrivyOperator.Dashboard.Application.Queries.Contexts.Abstractions;

namespace TrivyOperator.Dashboard.Api.K8s.Controllers;

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
