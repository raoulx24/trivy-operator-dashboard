using Microsoft.AspNetCore.Mvc;
using TrivyOperator.Dashboard.Application.Kubernetes.Queries.Namespaces.Services.Abstractions;

namespace TrivyOperator.Dashboard.Api.Kubernetes.Controllers;

[ApiController]
[Route("api/kubernetes-namespaces")]
public class KubernetesNamespacesController(IKubernetesNamespaceService kubernetesKubernetesNamespaceService)
    : ControllerBase
{
    [HttpGet(Name = "GetAllNamespaces")]
    [ProducesResponseType(typeof(IEnumerable<string>), StatusCodes.Status200OK, "application/json")]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError, "application/problem+json")]
    public async Task<IActionResult> GetAll(CancellationToken ctx) =>
        Ok(await kubernetesKubernetesNamespaceService.GetKubernetesNamespaces(ctx));
}
