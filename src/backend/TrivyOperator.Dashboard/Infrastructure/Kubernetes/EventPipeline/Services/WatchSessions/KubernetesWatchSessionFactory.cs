using k8s;
using k8s.Models;
using Microsoft.Extensions.Options;
using TrivyOperator.Dashboard.Application.Kubernetes.WatcherState.Abstractions;
using TrivyOperator.Dashboard.Application.Kubernetes.WatcherState.Options;
using TrivyOperator.Dashboard.Domain.Kubernetes.ValueObjects;
using TrivyOperator.Dashboard.Infrastructure.Kubernetes.EventPipeline.Services.EventPublishers.Abstractions;
using TrivyOperator.Dashboard.Infrastructure.Kubernetes.EventPipeline.Services.ResourceWatches.Abstractions;
using TrivyOperator.Dashboard.Infrastructure.Kubernetes.EventPipeline.Services.WatchSessions.Abstractions;
using TrivyOperator.Dashboard.Infrastructure.Kubernetes.EventPipeline.Services.WatchSessions.Failures.Abstractions;

namespace TrivyOperator.Dashboard.Infrastructure.Kubernetes.EventPipeline.Services.WatchSessions;

public sealed class KubernetesWatchSessionFactory<TKubernetesObjectList, TKubernetesObject>(
    IKubernetesResourceWatch<TKubernetesObjectList, TKubernetesObject> resourceWatch,
    IKubernetesEventPublisher<TKubernetesObject> eventPublisher,
    IKubernetesWatchSessionThrottle throttle,
    IWatchFailureClassifier failureClassifier,
    IWatchRetryPolicy retryPolicy,
    IKnownKubernetesTypeFactory knownKubernetesTypeFactory,
    IOptions<WatchersOptions> options,
    ILogger<KubernetesWatchSession<TKubernetesObjectList, TKubernetesObject>> logger
) : IKubernetesWatchSessionFactory<TKubernetesObjectList, TKubernetesObject>
    where TKubernetesObject : class, IKubernetesObject<V1ObjectMeta>, new()
    where TKubernetesObjectList : IKubernetesObject<V1ListMeta>, IItems<TKubernetesObject>
{
    public KubernetesWatchSession<TKubernetesObjectList, TKubernetesObject> Create(
        WatcherScope scope
    ) =>
        new(
            scope,
            resourceWatch,
            eventPublisher,
            throttle,
            failureClassifier,
            retryPolicy,
            knownKubernetesTypeFactory,
            options,
            logger
        );
}
