using k8s;
using k8s.Models;

namespace TrivyOperator.Dashboard.Infrastructure.Kubernetes.EventPipeline.Services.EventDispatchers.Abstractions;

public interface IKubernetesEventDispatcher<TKubernetesObject>
    where TKubernetesObject : IKubernetesObject<V1ObjectMeta>
{
    void StartEventsProcessing(CancellationToken ctx = default);
}
