using TrivyOperator.Dashboard.Domain.ScanOnDemand.ValueObjects;

namespace TrivyOperator.Dashboard.Application.ScanOnDemand.Abstractions;

// cancel requests waiting for the runner: the runner is the only one that changes pending / running scans and their
// pods, so the API only records the request. In memory (single replica): a request is lost on a restart
public interface IOnDemandScanCancellations
{
    void Request(ScanId id);

    // true (and removed) when a cancel was requested for the scan
    bool TryTake(ScanId id);
}
