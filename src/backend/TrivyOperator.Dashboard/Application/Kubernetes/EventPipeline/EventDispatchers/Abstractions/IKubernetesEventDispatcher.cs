using TrivyOperator.Dashboard.Application.Kubernetes.Models;
using TrivyOperator.Dashboard.Application.Shared.EventPipeline.EventDispatchers.Abstractions;
using TrivyOperator.Dashboard.Domain.Shared.Abstractions;

namespace TrivyOperator.Dashboard.Application.Kubernetes.EventPipeline.EventDispatchers.Abstractions;

public interface IKubernetesEventDispatcher<TResource, TKey> : IEventDispatcher<KubernetesEvent<TResource, TKey>>
    where TResource : class, IEntity<TKey>;
