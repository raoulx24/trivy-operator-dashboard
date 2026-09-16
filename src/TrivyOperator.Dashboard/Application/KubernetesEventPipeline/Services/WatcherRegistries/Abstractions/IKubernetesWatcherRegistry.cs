using TrivyOperator.Dashboard.Application.KubernetesEventPipeline.Models.WatcherEvents;

namespace TrivyOperator.Dashboard.Application.KubernetesEventPipeline.Services.WatcherRegistries.Abstractions;

public interface IKubernetesWatcherRegistry
{
    void StartWatcher(
        WatcherKey key,
        CancellationToken cancellationToken = default
    );

    Task StopWatcher(
        WatcherKey key,
        CancellationToken cancellationToken = default
    );

    Task RecreateWatcher(
        WatcherKey key,
        CancellationToken cancellationToken = default
    );
    
    Task Reconcile(
        IReadOnlyCollection<WatcherKey> desiredKeys,
        CancellationToken cancellationToken = default
    );
}
