using k8s;
using k8s.Models;
using TrivyOperator.Dashboard.Application.KubernetesEventPipeline.Models.WatcherEvents;
using TrivyOperator.Dashboard.Application.KubernetesEventPipeline.Services.BackgroundQueues.Abstractions;
using TrivyOperator.Dashboard.Application.KubernetesEventPipeline.Services.EventPublishers.Abstractions;

namespace TrivyOperator.Dashboard.Application.KubernetesEventPipeline.Services.EventPublishers;

public sealed class KubernetesEventPublisher<TKubernetesObject>(
    IKubernetesBackgroundQueue<TKubernetesObject> backgroundQueue,
    ILogger<KubernetesEventPublisher<TKubernetesObject>> logger
) : IKubernetesEventPublisher<TKubernetesObject>
    where TKubernetesObject : class, IKubernetesObject<V1ObjectMeta>, new()
{
    public async Task Publish(
        WatcherKey key,
        WatcherEventType eventType,
        CancellationToken cancellationToken,
        TKubernetesObject? kubernetesObject = null,
        Exception? exception = null
    )
    {
        logger.LogDebug(
            "Sending to Queue - {kubernetesObjectType} - {watcherEventType} - {key} - {kubernetesObjectName}",
            typeof(TKubernetesObject).Name,
            eventType,
            key,
            kubernetesObject?.Metadata?.Name ?? "N/A"
        );

        try
        {
            WatcherEvent<TKubernetesObject> watcherEvent = new()
            {
                Key = key,
                KubernetesObject = kubernetesObject,
                WatcherEventType = eventType,
                Exception = exception,
            };

            await backgroundQueue.QueueBackgroundWorkItemAsync(watcherEvent, cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogError(
                ex,
                "Watcher for {kubernetesObjectType} and key {key} could not enqueue {watcherEventType} - {exceptionMessage}",
                typeof(TKubernetesObject).Name,
                key,
                eventType,
                ex.Message
            );
        }
    }
}