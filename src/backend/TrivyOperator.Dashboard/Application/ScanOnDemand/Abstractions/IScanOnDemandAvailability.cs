namespace TrivyOperator.Dashboard.Application.ScanOnDemand.Abstractions;

// whether the composed feature can run: set once by the startup checks (namespace, pod template, permissions);
// read by the runner, the query service and the backend settings
public interface IScanOnDemandAvailability
{
    bool IsAvailable { get; }

    // why it is not available; null while available
    string? Reason { get; }

    void SetUnavailable(string reason);
}
