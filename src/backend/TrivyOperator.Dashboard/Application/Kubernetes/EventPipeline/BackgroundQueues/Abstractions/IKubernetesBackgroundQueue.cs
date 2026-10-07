using TrivyOperator.Dashboard.Application.Kubernetes.Models;
using TrivyOperator.Dashboard.Application.Shared.BackgroundQueues.Abstractions;
using TrivyOperator.Dashboard.Domain.Shared.Abstractions;

namespace TrivyOperator.Dashboard.Application.Kubernetes.EventPipeline.BackgroundQueues.Abstractions;

public interface IKubernetesBackgroundQueue<TResource, TKey> : IBackgroundQueue<KubernetesEvent<TResource, TKey>>
    where TResource : class, IEntity<TKey>;
