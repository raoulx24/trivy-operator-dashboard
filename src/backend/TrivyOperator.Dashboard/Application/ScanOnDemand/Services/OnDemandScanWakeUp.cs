using TrivyOperator.Dashboard.Application.ScanOnDemand.Abstractions;

namespace TrivyOperator.Dashboard.Application.ScanOnDemand.Services;

// several signals before the runner waits again count as one
public sealed class OnDemandScanWakeUp : IOnDemandScanWakeUp, IDisposable
{
    private readonly SemaphoreSlim signal = new(0, 1);

    public void Signal()
    {
        try
        {
            signal.Release();
        }
        catch (SemaphoreFullException)
        {
            // already signaled
        }
    }

    public Task<bool> WaitAsync(TimeSpan timeout, CancellationToken ctx = default) => signal.WaitAsync(timeout, ctx);

    public void Dispose() => signal.Dispose();
}
