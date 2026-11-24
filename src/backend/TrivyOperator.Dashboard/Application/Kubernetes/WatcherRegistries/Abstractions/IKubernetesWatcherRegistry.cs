using TrivyOperator.Dashboard.Domain.Kubernetes.ValueObjects;

namespace TrivyOperator.Dashboard.Application.Kubernetes.WatcherRegistries.Abstractions;

// start, stop and recreate are serialized per scope: a start waits until a running stop of the same scope has
// finished, including its Flushed event, so the old session's events always reach the pipeline before the new one's
public interface IKubernetesWatcherRegistry
{
    Task StartWatcher(WatcherScope scope, CancellationToken ctx = default);

    Task StopWatcher(WatcherScope scope, CancellationToken ctx = default);

    Task RecreateWatcher(WatcherScope scope, CancellationToken ctx = default);

    Type WatchedKubernetesObjectType { get; }
}
