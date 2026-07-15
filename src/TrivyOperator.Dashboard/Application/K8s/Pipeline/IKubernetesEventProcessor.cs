using k8s;
using k8s.Models;

namespace TrivyOperator.Dashboard.Application.K8s.Pipeline;

public interface IKubernetesEventProcessor<TKubernetesObject>
    where TKubernetesObject : IKubernetesObject<V1ObjectMeta>
{
    Task ProcessKubernetesEvent(IWatcherEvent<TKubernetesObject> watcherEvent, CancellationToken ctx);
}
