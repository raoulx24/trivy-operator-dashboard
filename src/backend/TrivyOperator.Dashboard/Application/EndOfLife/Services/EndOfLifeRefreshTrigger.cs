using TrivyOperator.Dashboard.Application.EndOfLife.Abstractions;

namespace TrivyOperator.Dashboard.Application.EndOfLife.Services;

// a single pending request is enough - multiple requests before the next wait collapse into one
public sealed class EndOfLifeRefreshTrigger : IEndOfLifeRefreshTrigger, IDisposable
{
    private readonly SemaphoreSlim signal = new(0, 1);

    public void RequestRebuild()
    {
        try
        {
            signal.Release();
        }
        catch (SemaphoreFullException)
        {
            // already requested
        }
    }

    public Task<bool> WaitAsync(TimeSpan timeout, CancellationToken ctx = default) => signal.WaitAsync(timeout, ctx);

    public void Dispose() => signal.Dispose();
}
