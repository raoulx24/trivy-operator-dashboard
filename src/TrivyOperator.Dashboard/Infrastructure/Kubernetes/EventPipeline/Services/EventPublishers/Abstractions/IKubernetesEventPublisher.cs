using k8s;
using k8s.Models;
using TrivyOperator.Dashboard.Application.Kubernetes.Models;
using TrivyOperator.Dashboard.Domain.Kubernetes.ValueObjects;

namespace TrivyOperator.Dashboard.Infrastructure.Kubernetes.EventPipeline.Services.EventPublishers.Abstractions;

public interface IKubernetesEventPublisher<in TKubernetesObject>
    where TKubernetesObject : class, IKubernetesObject<V1ObjectMeta>, new()
{
    Task Publish(
        ResourceLocation key,
        WatcherEventType eventType,
        CancellationToken ctx,
        TKubernetesObject? kubernetesObject = null,
        Exception? exception = null
    );
}
