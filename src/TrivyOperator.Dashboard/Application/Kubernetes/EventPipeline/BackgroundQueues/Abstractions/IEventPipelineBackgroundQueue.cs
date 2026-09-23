using TrivyOperator.Dashboard.Application.Kubernetes.Models;
using TrivyOperator.Dashboard.Domain.Shared.Abstractions;

namespace TrivyOperator.Dashboard.Application.Kubernetes.EventPipeline.BackgroundQueues.Abstractions;

public interface IEventPipelineBackgroundQueue<TResource, TKey> : IBackgroundQueue<WatcherEvent<TResource, TKey>>
    where TResource : class, IEntity<TKey>;
