using k8s;
using k8s.Models;
using TrivyOperator.Dashboard.Application.Kubernetes.EventPipeline.BackgroundQueues.Abstractions;
using TrivyOperator.Dashboard.Application.Kubernetes.Models;
using TrivyOperator.Dashboard.Application.Shared.Models;
using TrivyOperator.Dashboard.Domain.Kubernetes.ValueObjects;
using TrivyOperator.Dashboard.Domain.Shared.Abstractions;
using TrivyOperator.Dashboard.Infrastructure.Kubernetes.EventPipeline.Services.EventPublishers.Abstractions;
using TrivyOperator.Dashboard.Infrastructure.Kubernetes.ResourceMaterializer.Abstractions;

namespace TrivyOperator.Dashboard.Infrastructure.Kubernetes.EventPipeline.Services.EventPublishers;

public sealed class KubernetesEventPublisher<TKubernetesObject, TResource, TKey>(
    IEventPipelineBackgroundQueue<TResource, TKey> backgroundQueue,
    IResourceMaterializer<TKubernetesObject, TResource, TKey> resourceMaterializer,
    ILogger<KubernetesEventPublisher<TKubernetesObject, TResource, TKey>> logger
) : IKubernetesEventPublisher<TKubernetesObject>
    where TKubernetesObject : class, IKubernetesObject<V1ObjectMeta>, new()
    where TResource : class, IEntity<TKey>
{
    public async Task Publish(
        ResourceLocation key,
        PipelineEventType eventType,
        CancellationToken ctx,
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
            KubernetesEvent<TResource, TKey> kubernetesEvent = new(
                Key: key,
                PipelineEventType: eventType,
                Resource: await resourceMaterializer.Materialize(kubernetesObject, ctx),
                ResourceId: kubernetesObject?.Metadata == null
                    ? null
                    : new Uid(kubernetesObject.Metadata.Uid),
                Exception: exception
            );

            await backgroundQueue.QueueBackgroundWorkItemAsync(kubernetesEvent, ctx);
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