using Microsoft.Extensions.Options;
using TrivyOperator.Dashboard.Application.Kubernetes.EventPipeline.BackgroundQueues.Abstractions;
using TrivyOperator.Dashboard.Application.Kubernetes.Models;
using TrivyOperator.Dashboard.Application.Shared.Models;
using TrivyOperator.Dashboard.Domain.Shared.Abstractions;

namespace TrivyOperator.Dashboard.Infrastructure.BackgroundQueues;

public class EventPipelineBackgroundQueue<TResource, TKey>(
    IOptions<BackgroundQueueOptions> options,
    ILogger<EventPipelineBackgroundQueue<TResource, TKey>> localLogger
) : BackgroundQueue<KubernetesEvent<TResource, TKey>>(options, localLogger),
    IEventPipelineBackgroundQueue<TResource, TKey>
    where TResource : class, IEntity<TKey>;