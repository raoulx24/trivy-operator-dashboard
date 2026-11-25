using TrivyOperator.Dashboard.Application.ScanOnDemand.Models;
using TrivyOperator.Dashboard.Application.ScanOnDemand.Queries.Models;

namespace TrivyOperator.Dashboard.Application.ScanOnDemand.Queries.Services.Abstractions;

// what the API and the backend settings use; always registered (the real one or the null one)
public interface IScanOnDemandQueryService
{
    // configured, composed and the startup checks passed
    bool IsEnabled { get; }

    // why it is not enabled although configured (composition or startup checks); null when enabled or not configured
    string? UnavailableReason { get; }

    // the ceiling from the settings (names); empty when not enabled
    IReadOnlyList<string> AllowedReportTypes { get; }

    Task<ScanSubmitResult> SubmitAsync(ScanSubmitRequestDto request, string? requestedBy, CancellationToken ctx = default);
    Task<IReadOnlyList<OnDemandScanDto>> GetScansAsync(CancellationToken ctx = default);
    // null for an unknown or malformed id
    Task<OnDemandScanDto?> GetScanAsync(string id, CancellationToken ctx = default);
    // null for an unknown or malformed digest
    Task<OnDemandScanResultDto?> GetResultAsync(string digest, CancellationToken ctx = default);
    Task DeleteResultAsync(string digest, CancellationToken ctx = default);
}
