using k8s;
using k8s.Models;
using TrivyOperator.Dashboard.Application.K8s.Models.WatcherEvents;

namespace TrivyOperator.Dashboard.Application.K8s.Pipeline;

public interface IKubernetesWatcher
{
    void StartWatcher(WatcherKey key, CancellationToken ctx = default);
    Task RecreateWatcher(WatcherKey key, CancellationToken ctx = default);
    Task StopWatcher(WatcherKey key, CancellationToken ctx = default);
    
    Type WatchedKubernetesObjectType { get; }
}
