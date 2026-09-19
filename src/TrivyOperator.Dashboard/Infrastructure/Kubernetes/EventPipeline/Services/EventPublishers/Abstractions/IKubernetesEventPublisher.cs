using k8s;
using k8s.Models;
using TrivyOperator.Dashboard.Domain.Kubernetes.ValueObjects;
using TrivyOperator.Dashboard.Infrastructure.Kubernetes.EventPipeline.Models;

namespace TrivyOperator.Dashboard.Infrastructure.Kubernetes.EventPipeline.Services.EventPublishers.Abstractions;

public interface IKubernetesEventPublisher<in TKubernetesObject>
    where TKubernetesObject : class, IKubernetesObject<V1ObjectMeta>, new()
{
    Task Publish(
        ResourceLocation key,
        WatcherEventType eventType,
        CancellationToken cancellationToken,
        TKubernetesObject? kubernetesObject = null,
        Exception? exception = null
    );
}
