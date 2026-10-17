using TrivyOperator.Dashboard.Application.EndOfLife.Abstractions;

namespace TrivyOperator.Dashboard.Application.EndOfLife.Services;

// used when the feature is disabled - there is no hosted service to wake up
public sealed class NullEndOfLifeRefreshTrigger : IEndOfLifeRefreshTrigger
{
    public bool RequestRebuild() => false;

    public Task<bool> WaitAsync(TimeSpan timeout, CancellationToken ctx = default) => Task.FromResult(false);
}
