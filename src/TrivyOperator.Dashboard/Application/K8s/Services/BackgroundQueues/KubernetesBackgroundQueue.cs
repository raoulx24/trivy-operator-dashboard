using k8s;
using k8s.Models;
using Microsoft.Extensions.Options;
using TrivyOperator.Dashboard.Application.K8s.Models.WatcherEvents;
using TrivyOperator.Dashboard.Application.K8s.Pipeline;
using TrivyOperator.Dashboard.Infrastructure.BackgroundQueues;

namespace TrivyOperator.Dashboard.Application.K8s.Services.BackgroundQueues;

public class KubernetesBackgroundQueue<TKubernetesObject>(
    IOptions<BackgroundQueueOptions> options,
    ILogger<KubernetesBackgroundQueue<TKubernetesObject>> localLogger
) : BackgroundQueue<WatcherEvent<TKubernetesObject>>(options, localLogger),
    IKubernetesBackgroundQueue<TKubernetesObject>
    where TKubernetesObject : IKubernetesObject<V1ObjectMeta>, new()
{
    protected override void LogQueue(WatcherEvent<TKubernetesObject> enqueuedObject) => localLogger.LogDebug(
        "Queueing Event {watcherEventType} - {kubernetesObjectType} - {kubernetesObjectName}",
        enqueuedObject.WatcherEventType,
        typeof(TKubernetesObject).Name,
        enqueuedObject.KubernetesObject?.Metadata?.Name
    );

    protected override void LogDequeue(WatcherEvent<TKubernetesObject> dequeuedObject) => Logger.LogDebug(
        "Queueing Event {watcherEventType} - {kubernetesObjectType} - {kubernetesObjectName}",
        dequeuedObject.WatcherEventType,
        typeof(TKubernetesObject).Name,
        dequeuedObject.KubernetesObject?.Metadata?.Name
    );
}
