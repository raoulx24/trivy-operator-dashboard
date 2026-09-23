using TrivyOperator.Dashboard.Application.Shared.EventDispatchers.Abstractions;
using TrivyOperator.Dashboard.Domain.Shared.Abstractions;

namespace TrivyOperator.Dashboard.Application.Kubernetes.EventPipeline.EventDispatchers.Abstractions;

public interface IKubernetesEventDispatcher<TResource, TKey> : IEventDispatcher<TResource>
    where TResource : class, IEntity<TKey>;
