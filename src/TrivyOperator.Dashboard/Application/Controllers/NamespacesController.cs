﻿using Microsoft.AspNetCore.Mvc;
using TrivyOperator.Dashboard.Application.Services.Abstractions;

namespace TrivyOperator.Dashboard.Application.Controllers;

[ApiController]
[Route("api/kubernetes-namespaces")]
public class NamespacesController(INamespaceService kubernetesNamespaceService) : ControllerBase
{
    [HttpGet(Name = "GetAllNamespaces")]
    [ProducesResponseType<IEnumerable<string>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status500InternalServerError)]
    public async Task<IEnumerable<string>> GetAll() => await kubernetesNamespaceService.GetKubernetesNamespaces();
}
