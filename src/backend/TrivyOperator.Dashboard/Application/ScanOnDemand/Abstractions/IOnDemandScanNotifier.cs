namespace TrivyOperator.Dashboard.Application.ScanOnDemand.Abstractions;

// tells the connected clients that the scans list changed (they read it again); no payload, so there is nothing to
// order against the initial load. Never throws: a failed push is logged and the scan goes on
public interface IOnDemandScanNotifier
{
    Task ScansChangedAsync(CancellationToken ctx = default);
}
