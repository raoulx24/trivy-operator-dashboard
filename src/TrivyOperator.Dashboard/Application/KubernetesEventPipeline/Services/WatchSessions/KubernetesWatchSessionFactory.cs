using k8s;
using k8s.Models;
using Microsoft.Extensions.Options;
using TrivyOperator.Dashboard.Application.KubernetesEventPipeline.Models.WatcherEvents;
using TrivyOperator.Dashboard.Application.KubernetesEventPipeline.Services.EventPublishers.Abstractions;
using TrivyOperator.Dashboard.Application.KubernetesEventPipeline.Services.Options;
using TrivyOperator.Dashboard.Application.KubernetesEventPipeline.Services.ResourceWatches.Abstractions;
using TrivyOperator.Dashboard.Application.KubernetesEventPipeline.Services.WatchSessions.Abstractions;

namespace TrivyOperator.Dashboard.Application.KubernetesEventPipeline.Services.WatchSessions;

public sealed class KubernetesWatchSessionFactory<TKubernetesObjectList, TKubernetesObject>(
    IKubernetesResourceWatch<TKubernetesObjectList, TKubernetesObject> resourceWatch,
    IKubernetesEventPublisher<TKubernetesObject> eventPublisher,
    IOptions<WatchersOptions> options,
    ILogger<KubernetesWatchSession<TKubernetesObjectList, TKubernetesObject>> logger
) : IKubernetesWatchSessionFactory<TKubernetesObjectList, TKubernetesObject>
    where TKubernetesObject : class, IKubernetesObject<V1ObjectMeta>, new()
    where TKubernetesObjectList : IKubernetesObject<V1ListMeta>, IItems<TKubernetesObject>
{
    public KubernetesWatchSession<TKubernetesObjectList, TKubernetesObject> Create(
        WatcherKey key
    ) =>
        new(
            key,
            resourceWatch,
            eventPublisher,
            options,
            logger
        );
    
}
