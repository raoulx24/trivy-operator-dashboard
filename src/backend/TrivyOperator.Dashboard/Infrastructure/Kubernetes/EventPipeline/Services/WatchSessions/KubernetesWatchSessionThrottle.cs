using Microsoft.Extensions.Options;
using TrivyOperator.Dashboard.Application.Kubernetes.WatcherState.Options;
using TrivyOperator.Dashboard.Infrastructure.Kubernetes.EventPipeline.Services.WatchSessions.Abstractions;

namespace TrivyOperator.Dashboard.Infrastructure.Kubernetes.EventPipeline.Services.WatchSessions;

public sealed class KubernetesWatchSessionThrottle(IOptions<WatchersOptions> options)
    : IKubernetesWatchSessionThrottle, IDisposable
{
    private readonly SemaphoreSlim gate = new(Math.Max(1, options.Value.MaxConcurrentInitialLists));

    public async Task<T> Run<T>(
        Func<CancellationToken, Task<T>> work,
        Func<Task> onThrottled,
        CancellationToken ctx = default
    )
    {
        if (!gate.Wait(0))
        {
            await onThrottled();
            await gate.WaitAsync(ctx);
        }

        try
        {
            return await work(ctx);
        }
        finally
        {
            gate.Release();
        }
    }

    public void Dispose()
    {
        gate.Dispose();
    }
}
