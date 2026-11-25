using TrivyOperator.Dashboard.Application.ScanOnDemand.Models;
using TrivyOperator.Dashboard.Application.ScanOnDemand.Queries.Models;
using TrivyOperator.Dashboard.Application.ScanOnDemand.Queries.Services.Abstractions;

namespace TrivyOperator.Dashboard.Application.ScanOnDemand.Queries.Services;

// the feature is not composed; unavailableReason: set when it is configured but cannot be composed (e.g. multi-context)
public sealed class ScanOnDemandQueryNullService(string? unavailableReason) : IScanOnDemandQueryService
{
    private const string NotEnabled = "Scan on demand is not enabled.";

    public bool IsEnabled => false;

    public string? UnavailableReason => unavailableReason;

    public IReadOnlyList<string> AllowedReportTypes => [];

    public Task<ScanSubmitResult> SubmitAsync(
        ScanSubmitRequestDto request,
        string? requestedBy,
        CancellationToken ctx = default)
        => Task.FromResult(new ScanSubmitResult(null, [unavailableReason ?? NotEnabled,]));

    public Task<IReadOnlyList<OnDemandScanDto>> GetScansAsync(CancellationToken ctx = default)
        => Task.FromResult<IReadOnlyList<OnDemandScanDto>>([]);

    public Task<OnDemandScanDto?> GetScanAsync(string id, CancellationToken ctx = default)
        => Task.FromResult<OnDemandScanDto?>(null);

    public Task<OnDemandScanResultDto?> GetResultAsync(string digest, CancellationToken ctx = default)
        => Task.FromResult<OnDemandScanResultDto?>(null);

    public Task DeleteResultAsync(string digest, CancellationToken ctx = default)
        => Task.CompletedTask;
}
