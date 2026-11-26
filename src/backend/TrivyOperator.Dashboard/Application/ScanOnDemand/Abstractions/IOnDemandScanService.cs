using TrivyOperator.Dashboard.Application.ScanOnDemand.Models;
using TrivyOperator.Dashboard.Domain.ScanOnDemand.Entities;
using TrivyOperator.Dashboard.Domain.ScanOnDemand.ValueObjects;
using TrivyOperator.Dashboard.Domain.Trivy.ValueObjects.Shared;

namespace TrivyOperator.Dashboard.Application.ScanOnDemand.Abstractions;

// what the API calls
public interface IOnDemandScanService
{
    // validates and queues the scan; it runs in the background
    Task<ScanSubmitResult> SubmitAsync(
        string? imageReference,
        IReadOnlyCollection<ScanReportType> types,
        string? requestedBy,
        CancellationToken ctx = default);

    Task<OnDemandScan?> GetScanAsync(ScanId id, CancellationToken ctx = default);
    // a pending or running scan: the request is recorded and the runner cancels it right after (pod deleted, no result)
    Task<ScanCancelResult> CancelScanAsync(ScanId id, CancellationToken ctx = default);
    // only a finished scan can be deleted (a pending or running one is cancelled first); its result stays
    Task<ScanDeleteResult> DeleteScanAsync(ScanId id, CancellationToken ctx = default);
    Task<IReadOnlyList<OnDemandScan>> GetScansAsync(CancellationToken ctx = default);
    Task<OnDemandScanResult?> GetResultAsync(Digest digest, CancellationToken ctx = default);
    // the delete from the inspect page, before the TTL
    Task DeleteResultAsync(Digest digest, CancellationToken ctx = default);
}
