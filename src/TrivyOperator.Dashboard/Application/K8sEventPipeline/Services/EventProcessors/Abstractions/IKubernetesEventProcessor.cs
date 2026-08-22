using k8s;
using k8s.Models;
using TrivyOperator.Dashboard.Application.K8sEventPipeline.Models.WatcherEvents;

namespace TrivyOperator.Dashboard.Application.K8sEventPipeline.Services.EventProcessors.Abstractions;

public interface IKubernetesEventProcessor<TKubernetesObject>
    where TKubernetesObject : IKubernetesObject<V1ObjectMeta>, new()
{
    Task ProcessKubernetesEvent(WatcherEvent<TKubernetesObject> watcherEvent, CancellationToken ctx);
}
