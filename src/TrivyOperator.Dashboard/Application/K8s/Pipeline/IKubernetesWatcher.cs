using k8s;
using k8s.Models;
using TrivyOperator.Dashboard.Application.K8s.Models.WatcherEvents;

namespace TrivyOperator.Dashboard.Application.K8s.Pipeline;

public interface IKubernetesWatcher
{
    Task Add(WatcherKey key, CancellationToken ctx = default);
    Task Recreate(WatcherKey key, CancellationToken ctx = default);
    Task Delete(WatcherKey key, CancellationToken ctx = default);
}

public interface IKubernetesWatcher<TKubernetesObject> : IKubernetesWatcher
    where TKubernetesObject : IKubernetesObject<V1ObjectMeta>
{
    
}