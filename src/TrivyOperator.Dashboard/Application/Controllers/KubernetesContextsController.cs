using Microsoft.AspNetCore.Mvc;
using TrivyOperator.Dashboard.Application.Models;
using TrivyOperator.Dashboard.Application.Services.KubernetesContexts.Abstractions;

namespace TrivyOperator.Dashboard.Application.Controllers;

[ApiController]
[Route("api/kubernetes-contexts")]
public class KubernetesContextsController(IKubernetesContextService kubernetesContextService)
{
    [HttpGet(Name = "GetKubernetesContexts")]
    [ProducesResponseType<IEnumerable<KubernetesContextsDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status500InternalServerError)]
    public async Task<KubernetesContextsDto> GetKubernetesContexts() => await kubernetesContextService.GetKubernetesContextsDto();
}
