namespace TrivyOperator.Dashboard.Infrastructure.Kubernetes.EventPipeline.Services.WatchSessions.Abstractions;

public interface IKubernetesWatchSessionThrottle
{
    // onThrottled is invoked once, before waiting, only when no slot is immediately available
    Task<T> Run<T>(
        Func<CancellationToken, Task<T>> work,
        Func<Task> onThrottled,
        CancellationToken ctx = default
    );
}
