namespace TrivyOperator.Dashboard.Domain.ScanOnDemand.ValueObjects;

public enum ScanState
{
    // accepted, the pod is not created yet
    Pending,
    // the pod is created, waiting for the reports
    Running,
    // the selected reports were collected (some may be missing: see OnDemandScan.ProducedReportTypes)
    Completed,
    // the pod was rejected or failed, or the reports could not be collected
    Failed,
    // no selected report arrived before the timeout
    TimedOut,
    // cancelled by a user while pending or running; no result is stored
    Cancelled,
}
