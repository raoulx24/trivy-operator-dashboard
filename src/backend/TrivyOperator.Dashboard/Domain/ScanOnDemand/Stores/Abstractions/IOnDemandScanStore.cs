using TrivyOperator.Dashboard.Domain.ScanOnDemand.Entities;
using TrivyOperator.Dashboard.Domain.ScanOnDemand.ValueObjects;
using TrivyOperator.Dashboard.Domain.Trivy.ValueObjects.Shared;

namespace TrivyOperator.Dashboard.Domain.ScanOnDemand.Stores.Abstractions;

public interface IOnDemandScanStore
{
    // insert or replace
    Task SaveScanAsync(OnDemandScan scan, CancellationToken ctx = default);
    Task<OnDemandScan?> GetScanAsync(ScanId id, CancellationToken ctx = default);
    // the scan records only, no reports
    Task<IReadOnlyList<OnDemandScan>> GetScansAsync(CancellationToken ctx = default);
    // removes the scan record only; the result of its digest (if any) stays
    Task DeleteScanAsync(OnDemandScan scan, CancellationToken ctx = default);

    // insert or replace; expires at result.ExpiresAt
    Task SaveResultAsync(OnDemandScanResult result, CancellationToken ctx = default);
    Task<OnDemandScanResult?> GetResultAsync(Digest digest, CancellationToken ctx = default);
    // removes the result and the scan records that point to the digest
    Task DeleteResultAsync(Digest digest, CancellationToken ctx = default);
}
