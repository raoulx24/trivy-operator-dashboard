using Microsoft.AspNetCore.Mvc;
using TrivyOperator.Dashboard.Application.K8s.Services.RawDomain;
using TrivyOperator.Dashboard.Application.K8s.Services.RawDomain.Abstracts;

namespace TrivyOperator.Dashboard.Application.K8s.Controllers;

[ApiController]
[Route("internal/raw")]
[ApiExplorerSettings(IgnoreApi = true)]
public sealed class RawController(ILogger<RawController> logger) : ControllerBase
{
    // /internal/raw?typeCr=VulnerabilityReportCr&key=trivy
    [HttpGet]
    public async Task<IResult> Get(
        [FromServices] IRawDomainQueryService svc,
        [FromQuery] string typeCr,
        [FromQuery] string key,
        CancellationToken ct
    )
    {
        if (string.IsNullOrWhiteSpace(typeCr) || string.IsNullOrWhiteSpace(key))
        {
            logger.LogError("typeCr or key is/are null or empty.");
            return Results.BadRequest("typeCr and key are required.");
        }

        Type? valueType = AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(a => a.GetTypes())
            .FirstOrDefault(t => t.Name == typeCr);
        if (valueType is null)
        {
            logger.LogError("Type '{TypeCr}' could not be resolved.", typeCr);
            return Results.BadRequest($"Cannot resolve typeCr '{typeCr}'.");
        }

        try
        {
            IReadOnlyList<object> result = await svc.GetAllAsync(valueType, key, ct);

            if (result.Count == 0)
            {
                logger.LogWarning("No data found for type '{TypeCr}' with key '{Key}'.", typeCr, key);
                return Results.NotFound();
            }

            return Results.Ok(result);
        }
        catch (CacheNotRegisteredException)
        {
            logger.LogError("Cache not registered for type '{TypeCr}'.", typeCr);
            return Results.NotFound();
        }
    }
}
