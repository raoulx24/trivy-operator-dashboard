using TrivyOperator.Dashboard.Application.KubernetesEventPipeline.Models.WatcherEvents;

namespace TrivyOperator.Dashboard.Application.KubernetesEventPipeline.Services.Watchers.Abstractions;

public interface IKubernetesWatcher
{
    void StartWatcher(WatcherKey key, CancellationToken ctx = default);
    Task RecreateWatcher(WatcherKey key, CancellationToken ctx = default);
    Task StopWatcher(WatcherKey key, CancellationToken ctx = default);
    
    Type WatchedKubernetesObjectType { get; }
}
