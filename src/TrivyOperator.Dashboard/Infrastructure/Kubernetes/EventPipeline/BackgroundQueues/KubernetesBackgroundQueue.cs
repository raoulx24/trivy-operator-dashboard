using Microsoft.Extensions.Options;
using TrivyOperator.Dashboard.Application.Kubernetes.EventPipeline.BackgroundQueues.Abstractions;
using TrivyOperator.Dashboard.Application.Kubernetes.Models;
using TrivyOperator.Dashboard.Domain.Shared.Abstractions;
using TrivyOperator.Dashboard.Infrastructure.BackgroundQueues;

namespace TrivyOperator.Dashboard.Infrastructure.Kubernetes.EventPipeline.BackgroundQueues;

public class KubernetesBackgroundQueue<TResource, TKey>(
    IOptions<BackgroundQueueOptions> options,
    ILogger<KubernetesBackgroundQueue<TResource, TKey>> localLogger
) : BackgroundQueue<KubernetesEvent<TResource, TKey>>(options, localLogger),
    IKubernetesBackgroundQueue<TResource, TKey>
    where TResource : class, IEntity<TKey>;