using k8s;
using k8s.Models;
using TrivyOperator.Dashboard.Application.K8s.Models.WatcherEvents;

namespace TrivyOperator.Dashboard.Application.K8s.Pipeline;

public interface IKubernetesEventProcessor<TKubernetesObject>
    where TKubernetesObject : IKubernetesObject<V1ObjectMeta>, new()
{
    Task ProcessKubernetesEvent(WatcherEvent<TKubernetesObject> watcherEvent, CancellationToken ctx);
}
