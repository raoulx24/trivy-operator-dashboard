using TrivyOperator.Dashboard.Application.Kubernetes.Models;
using TrivyOperator.Dashboard.Application.Shared.EventProcessors.Abstractions;
using TrivyOperator.Dashboard.Application.Shared.Models;
using TrivyOperator.Dashboard.Domain.Shared.Abstractions;

namespace TrivyOperator.Dashboard.Application.Kubernetes.EventPipeline.EventProcessors.Abstractions;

public interface IKubernetesEventProcessor<TResource, TKey> : IEventProcessor<KubernetesEvent<TResource, TKey>>
    where TResource : class, IEntity<TKey>;
