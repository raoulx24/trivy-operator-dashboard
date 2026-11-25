namespace TrivyOperator.Dashboard.Application.ScanOnDemand.Abstractions;

// lets a submit start the runner's next tick right away instead of waiting for the poll interval
public interface IOnDemandScanWakeUp
{
    void Signal();

    // true when signaled, false when the timeout passed
    Task<bool> WaitAsync(TimeSpan timeout, CancellationToken ctx = default);
}
