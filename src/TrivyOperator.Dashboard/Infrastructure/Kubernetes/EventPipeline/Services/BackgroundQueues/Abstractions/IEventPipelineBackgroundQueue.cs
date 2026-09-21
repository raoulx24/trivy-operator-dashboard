using TrivyOperator.Dashboard.Application.Kubernetes.Models;
using TrivyOperator.Dashboard.Domain.Shared.Abstractions;

namespace TrivyOperator.Dashboard.Infrastructure.Kubernetes.EventPipeline.Services.BackgroundQueues.Abstractions;

public interface IEventPipelineBackgroundQueue<TResource, TKey> : IBackgroundQueue<WatcherEvent<TResource, TKey>>
    where TResource : class, IEntity<TKey>;
