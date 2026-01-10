using k8s;
using k8s.Models;
using TrivyOperator.Dashboard.Application.K8s.Services.WatcherEvents.Abstractions;

namespace TrivyOperator.Dashboard.Application.K8s.Services.EventDispatchers.Abstractions;

public interface IKubernetesEventProcessor<TKubernetesObject>
    where TKubernetesObject : IKubernetesObject<V1ObjectMeta>
{
    public Task ProcessKubernetesEvent(IWatcherEvent<TKubernetesObject> watcherEvent, CancellationToken cancellationToken);
}
