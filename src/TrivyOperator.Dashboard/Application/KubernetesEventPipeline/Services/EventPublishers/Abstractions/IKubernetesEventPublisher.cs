using k8s;
using k8s.Models;
using TrivyOperator.Dashboard.Application.KubernetesEventPipeline.Models.WatcherEvents;

namespace TrivyOperator.Dashboard.Application.KubernetesEventPipeline.Services.EventPublishers.Abstractions;

public interface IKubernetesEventPublisher<in TKubernetesObject>
    where TKubernetesObject : class, IKubernetesObject<V1ObjectMeta>, new()
{
    Task Publish(
        WatcherKey key,
        WatcherEventType eventType,
        CancellationToken cancellationToken,
        TKubernetesObject? kubernetesObject = null,
        Exception? exception = null
    );
}
