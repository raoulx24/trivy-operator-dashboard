using Microsoft.AspNetCore.Mvc;
using TrivyOperator.Dashboard.Application.EndOfLife.Queries.Models;
using TrivyOperator.Dashboard.Application.EndOfLife.Queries.Services.Abstractions;
using TrivyOperator.Dashboard.Domain.EndOfLife.ValueObjects;

namespace TrivyOperator.Dashboard.Api.EndOfLife.Controllers;

[ApiController]
[Route("api/end-of-life")]
public class EndOfLifeController(IEndOfLifeService endOfLifeService) : ControllerBase
{
    [HttpGet("os", Name = "GetOsEndOfLife")]
    [ProducesResponseType(typeof(IReadOnlyList<EolInfoDto>), StatusCodes.Status200OK, "application/json")]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError, "application/problem+json")]
    public async Task<IActionResult> GetAllOs(CancellationToken ctx) =>
        Ok(await endOfLifeService.GetAll(EolSubjectKind.Os, ctx));

    [HttpGet("os/{family}", Name = "GetOsEndOfLifeByFamily")]
    [ProducesResponseType(typeof(IReadOnlyList<EolInfoDto>), StatusCodes.Status200OK, "application/json")]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError, "application/problem+json")]
    public async Task<IActionResult> GetOsByFamily(string family, CancellationToken ctx) =>
        Ok(await endOfLifeService.GetByName(EolSubjectKind.Os, family, ctx));

    [HttpGet("os/{family}/{version}", Name = "GetOsEndOfLifeByFamilyAndVersion")]
    [ProducesResponseType(typeof(EolInfoDto), StatusCodes.Status200OK, "application/json")]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError, "application/problem+json")]
    public async Task<IActionResult> GetOsByFamilyAndVersion(string family, string version, CancellationToken ctx)
    {
        EolInfoDto? result = await endOfLifeService.GetByNameAndVersion(EolSubjectKind.Os, family, version, ctx);

        return result is null ? NotFound() : Ok(result);
    }

    [HttpPost("rebuild", Name = "RebuildEndOfLife")]
    [ProducesResponseType(StatusCodes.Status202Accepted)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict, "application/problem+json")]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError, "application/problem+json")]
    public IActionResult Rebuild() =>
        endOfLifeService.RequestRebuild()
            ? Accepted()
            : Problem(detail: "End of life support is disabled.", statusCode: StatusCodes.Status409Conflict);
}
