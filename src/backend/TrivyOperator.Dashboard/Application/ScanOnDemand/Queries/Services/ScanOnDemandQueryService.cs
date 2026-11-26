using TrivyOperator.Dashboard.Application.ScanOnDemand.Abstractions;
using TrivyOperator.Dashboard.Application.ScanOnDemand.Models;
using TrivyOperator.Dashboard.Application.ScanOnDemand.Queries.Mappers;
using TrivyOperator.Dashboard.Application.ScanOnDemand.Queries.Models;
using TrivyOperator.Dashboard.Application.ScanOnDemand.Queries.Services.Abstractions;
using TrivyOperator.Dashboard.Domain.ScanOnDemand.Entities;
using TrivyOperator.Dashboard.Domain.ScanOnDemand.ValueObjects;
using TrivyOperator.Dashboard.Domain.Trivy.ValueObjects.Shared;

namespace TrivyOperator.Dashboard.Application.ScanOnDemand.Queries.Services;

public sealed class ScanOnDemandQueryService(
    IOnDemandScanService service,
    IScanRequestValidator validator,
    IScanOnDemandAvailability availability
) : IScanOnDemandQueryService
{
    public bool IsEnabled => availability.IsAvailable;

    public string? UnavailableReason => availability.Reason;

    public IReadOnlyList<string> AllowedReportTypes
        => IsEnabled ? [.. validator.AllowedReportTypes.Order().Select(static x => x.ToString()),] : [];

    public Task<ScanSubmitResult> SubmitAsync(
        ScanSubmitRequestDto request,
        string? requestedBy,
        CancellationToken ctx = default)
    {
        List<ScanReportType> types = [];
        List<string> errors = [];

        foreach (string name in request.ReportTypes ?? [])
        {
            if (Enum.TryParse(name, true, out ScanReportType type) && Enum.IsDefined(type))
            {
                types.Add(type);
            }
            else
            {
                errors.Add($"Unknown report type '{name}'.");
            }
        }

        return errors.Count > 0
            ? Task.FromResult(new ScanSubmitResult(null, errors))
            : service.SubmitAsync(request.Image, types, requestedBy, ctx);
    }

    public async Task<IReadOnlyList<OnDemandScanDto>> GetScansAsync(CancellationToken ctx = default)
        => [.. (await service.GetScansAsync(ctx)).OrderByDescending(static x => x.CreatedAt).Select(static x => x.ToDto()),];

    public async Task<OnDemandScanDto?> GetScanAsync(string id, CancellationToken ctx = default)
    {
        ScanId scanId = new(id);

        return scanId.IsValid ? (await service.GetScanAsync(scanId, ctx))?.ToDto() : null;
    }

    public Task<ScanCancelResult> CancelScanAsync(string id, CancellationToken ctx = default)
    {
        ScanId scanId = new(id);

        return scanId.IsValid ? service.CancelScanAsync(scanId, ctx) : Task.FromResult(ScanCancelResult.NotFound);
    }

    public Task<ScanDeleteResult> DeleteScanAsync(string id, CancellationToken ctx = default)
    {
        ScanId scanId = new(id);

        return scanId.IsValid ? service.DeleteScanAsync(scanId, ctx) : Task.FromResult(ScanDeleteResult.NotFound);
    }

    public async Task<OnDemandScanResultDto?> GetResultAsync(string digest, CancellationToken ctx = default)
    {
        Digest key = new(digest);

        if (!key.IsValid)
        {
            return null;
        }

        OnDemandScanResult? result = await service.GetResultAsync(key, ctx);

        if (result is null)
        {
            return null;
        }

        OnDemandScan? lastScan = result.LastScanId.IsValid ? await service.GetScanAsync(result.LastScanId, ctx) : null;

        return result.ToDto(lastScan?.Image.FullName);
    }

    public Task DeleteResultAsync(string digest, CancellationToken ctx = default)
    {
        Digest key = new(digest);

        return key.IsValid ? service.DeleteResultAsync(key, ctx) : Task.CompletedTask;
    }
}
