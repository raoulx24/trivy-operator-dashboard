using TrivyOperator.Dashboard.Application.K8sEventPipeline.Models.WatcherEvents;

namespace TrivyOperator.Dashboard.Application.K8sEventPipeline.Services.Watchers.Abstractions;

public interface IKubernetesWatcher
{
    void StartWatcher(WatcherKey key, CancellationToken ctx = default);
    Task RecreateWatcher(WatcherKey key, CancellationToken ctx = default);
    Task StopWatcher(WatcherKey key, CancellationToken ctx = default);
    
    Type WatchedKubernetesObjectType { get; }
}
