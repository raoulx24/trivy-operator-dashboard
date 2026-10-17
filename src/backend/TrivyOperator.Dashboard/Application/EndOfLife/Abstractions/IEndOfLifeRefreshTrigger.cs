namespace TrivyOperator.Dashboard.Application.EndOfLife.Abstractions;

// lets the API wake up the hosted service and force a refetch of all products, ignoring their TTLs
public interface IEndOfLifeRefreshTrigger
{
    // false when there is nothing to trigger (feature disabled)
    bool RequestRebuild();

    // waits until either the timeout elapses or a rebuild is requested
    // returns true if a rebuild was requested (and consumes the request)
    Task<bool> WaitAsync(TimeSpan timeout, CancellationToken ctx = default);
}
