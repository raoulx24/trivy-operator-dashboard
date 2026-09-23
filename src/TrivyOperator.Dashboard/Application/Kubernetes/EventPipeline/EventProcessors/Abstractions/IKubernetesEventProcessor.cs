using TrivyOperator.Dashboard.Application.Kubernetes.Models;
using TrivyOperator.Dashboard.Domain.Shared.Abstractions;

namespace TrivyOperator.Dashboard.Application.Kubernetes.EventPipeline.EventProcessors.Abstractions;

public interface IKubernetesEventProcessor<TResource, TKey>
    where TResource : class, IEntity<TKey>
{
    Task ProcessKubernetesEvent(WatcherEvent<TResource, TKey> watcherEvent, CancellationToken ctx);
}
