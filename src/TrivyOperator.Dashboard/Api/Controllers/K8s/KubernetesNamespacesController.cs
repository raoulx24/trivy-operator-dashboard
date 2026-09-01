using Microsoft.AspNetCore.Mvc;
using TrivyOperator.Dashboard.Application.Queries.Namespaces.Services.Abstractions;

namespace TrivyOperator.Dashboard.Api.Controllers.K8s;

[ApiController]
[Route("api/kubernetes-namespaces")]
public class KubernetesNamespacesController(IKubernetesNamespaceService kubernetesKubernetesNamespaceService)
    : ControllerBase
{
    [HttpGet(Name = "GetAllNamespaces")]
    [ProducesResponseType<IEnumerable<string>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status500InternalServerError)]
    public async Task<IEnumerable<string>> GetAll(CancellationToken ctx) =>
        await kubernetesKubernetesNamespaceService.GetKubernetesNamespaces(ctx);
}
