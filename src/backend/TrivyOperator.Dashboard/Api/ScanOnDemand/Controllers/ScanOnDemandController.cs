using Microsoft.AspNetCore.Mvc;
using TrivyOperator.Dashboard.Application.ScanOnDemand.Models;
using TrivyOperator.Dashboard.Application.ScanOnDemand.Queries.Models;
using TrivyOperator.Dashboard.Application.ScanOnDemand.Queries.Services.Abstractions;

namespace TrivyOperator.Dashboard.Api.ScanOnDemand.Controllers;

// every endpoint answers 404 (ProblemDetails) while the feature is not enabled
[ApiController]
[Route("api/scan-on-demand")]
public class ScanOnDemandController(IScanOnDemandQueryService scanOnDemandQueryService) : ControllerBase
{
    [HttpPost("scans", Name = "SubmitOnDemandScan")]
    [ProducesResponseType(typeof(ScanSubmitResponseDto), StatusCodes.Status202Accepted, "application/json")]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest, "application/problem+json")]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound, "application/problem+json")]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError, "application/problem+json")]
    public async Task<IActionResult> Submit([FromBody] ScanSubmitRequestDto? request, CancellationToken ct)
    {
        if (!scanOnDemandQueryService.IsEnabled)
        {
            return NotEnabled();
        }

        if (request is null)
        {
            return Problem(
                statusCode: StatusCodes.Status400BadRequest,
                title: "No info provided",
                detail: "An image and at least one report type must be provided.");
        }

        // requestedBy: filled from the user's claims once authentication is in place
        ScanSubmitResult result = await scanOnDemandQueryService.SubmitAsync(request, null, ct);

        if (!result.IsAccepted)
        {
            return Problem(
                statusCode: StatusCodes.Status400BadRequest,
                title: "Scan not accepted",
                detail: string.Join(" ", result.Errors));
        }

        return Accepted(new ScanSubmitResponseDto(result.Id!.Value.Value, result.IsExisting));
    }

    [HttpGet("scans", Name = "GetOnDemandScans")]
    [ProducesResponseType(typeof(IReadOnlyList<OnDemandScanDto>), StatusCodes.Status200OK, "application/json")]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound, "application/problem+json")]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError, "application/problem+json")]
    public async Task<IActionResult> GetScans(CancellationToken ct)
    {
        if (!scanOnDemandQueryService.IsEnabled)
        {
            return NotEnabled();
        }

        return Ok(await scanOnDemandQueryService.GetScansAsync(ct));
    }

    [HttpGet("scans/{id}", Name = "GetOnDemandScan")]
    [ProducesResponseType(typeof(OnDemandScanDto), StatusCodes.Status200OK, "application/json")]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound, "application/problem+json")]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError, "application/problem+json")]
    public async Task<IActionResult> GetScan(string id, CancellationToken ct)
    {
        if (!scanOnDemandQueryService.IsEnabled)
        {
            return NotEnabled();
        }

        OnDemandScanDto? scan = await scanOnDemandQueryService.GetScanAsync(id, ct);

        return scan is null
            ? Problem(statusCode: StatusCodes.Status404NotFound, title: "Not found", detail: $"Scan '{id}' was not found.")
            : Ok(scan);
    }

    [HttpGet("results/{digest}", Name = "GetOnDemandScanResult")]
    [ProducesResponseType(typeof(OnDemandScanResultDto), StatusCodes.Status200OK, "application/json")]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound, "application/problem+json")]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError, "application/problem+json")]
    public async Task<IActionResult> GetResult(string digest, CancellationToken ct)
    {
        if (!scanOnDemandQueryService.IsEnabled)
        {
            return NotEnabled();
        }

        OnDemandScanResultDto? result = await scanOnDemandQueryService.GetResultAsync(digest, ct);

        return result is null
            ? Problem(statusCode: StatusCodes.Status404NotFound, title: "Not found", detail: $"No scan result for '{digest}'.")
            : Ok(result);
    }

    [HttpDelete("results/{digest}", Name = "DeleteOnDemandScanResult")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound, "application/problem+json")]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError, "application/problem+json")]
    public async Task<IActionResult> DeleteResult(string digest, CancellationToken ct)
    {
        if (!scanOnDemandQueryService.IsEnabled)
        {
            return NotEnabled();
        }

        await scanOnDemandQueryService.DeleteResultAsync(digest, ct);

        return NoContent();
    }

    private IActionResult NotEnabled()
        => Problem(
            statusCode: StatusCodes.Status404NotFound,
            title: "Scan on demand is not enabled",
            detail: scanOnDemandQueryService.UnavailableReason ?? "Scan on demand is not enabled in the settings.");
}
