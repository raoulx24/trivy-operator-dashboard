using Microsoft.AspNetCore.Mvc;
using TrivyOperator.Dashboard.Application.K8s.Services.Namespaces.Abstractions;

namespace TrivyOperator.Dashboard.Application.K8s.Controllers;

[ApiController]
[Route("api/kubernetes-namespaces")]
public class KubernetesNamespacesController(IKubernetesNamespaceService kubernetesKubernetesNamespaceService) : ControllerBase
{
    [HttpGet(Name = "GetAllNamespaces")]
    [ProducesResponseType<IEnumerable<string>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status500InternalServerError)]
    public async Task<IEnumerable<string>> GetAll() => await kubernetesKubernetesNamespaceService.GetKubernetesNamespaces();
}
