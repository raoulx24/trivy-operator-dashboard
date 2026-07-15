using k8s;
using k8s.Models;

namespace TrivyOperator.Dashboard.Application.K8s.Pipeline;

public interface IKubernetesEventDispatcher<TKubernetesObject>
    where TKubernetesObject : IKubernetesObject<V1ObjectMeta>
{
    bool IsQueueProcessingStarted { get; }
    void StartEventsProcessing(CancellationToken cancellationToken);
}
