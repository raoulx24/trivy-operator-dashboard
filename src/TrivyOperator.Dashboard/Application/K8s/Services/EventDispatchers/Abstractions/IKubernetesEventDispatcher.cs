using k8s;
using k8s.Models;

namespace TrivyOperator.Dashboard.Application.K8s.Services.EventDispatchers.Abstractions;

public interface IKubernetesEventDispatcher<TKubernetesObject>
    where TKubernetesObject : IKubernetesObject<V1ObjectMeta>
{
    void StartEventsProcessing(CancellationToken cancellationToken);
    bool IsQueueProcessingStarted { get; }
}