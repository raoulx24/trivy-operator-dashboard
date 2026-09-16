using TrivyOperator.Dashboard.Application.KubernetesEventPipeline.Models.WatcherEvents;

namespace TrivyOperator.Dashboard.Application.KubernetesEventPipeline.Services.WatcherRegistries.Abstractions;

public interface IKubernetesWatcherRegistry
{
    void StartWatcher(WatcherKey key, CancellationToken ctx = default);

    Task StopWatcher(WatcherKey key, CancellationToken ctx = default);

    Task RecreateWatcher(WatcherKey key, CancellationToken ctx = default);
}
