using TrivyOperator.Dashboard.Application.ScanOnDemand.Abstractions;

namespace TrivyOperator.Dashboard.Application.ScanOnDemand.Services;

// available until a startup check says otherwise; only ever goes from available to unavailable
public sealed class ScanOnDemandAvailability : IScanOnDemandAvailability
{
    private volatile string? reason;

    public bool IsAvailable => reason is null;

    public string? Reason => reason;

    public void SetUnavailable(string reason) => this.reason ??= reason;
}
