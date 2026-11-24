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
    IKubernetesBackgroundQueue<TResource, TKey> backgroundQueue,
    IResourceMaterializer<TKubernetesObject, TResource, TKey> resourceMaterializer,
    ILogger<KubernetesEventPublisher<TKubernetesObject, TResource, TKey>> logger
) : IKubernetesEventPublisher<TKubernetesObject>
    where TKubernetesObject : class, IKubernetesObject<V1ObjectMeta>, new()
    where TResource : class, IEntity<TKey>
{
    public async Task Publish(
        WatcherScope scope,
        PipelineEventType eventType,
        CancellationToken ctx,
        TKubernetesObject? kubernetesObject = null,
        WatchFailure? failure = null,
        MalformedObject? malformed = null
    )
    {
        logger.LogDebug(
            "Sending to Queue - {kubernetesObjectType} - {watcherEventType} - {scope} - {kubernetesObjectName}",
            typeof(TKubernetesObject).Name,
            eventType,
            scope,
            kubernetesObject?.Metadata?.Name ?? malformed?.Name ?? "N/A"
        );

        TResource? resource = null;
        Uid? resourceId = null;

        if (kubernetesObject?.Metadata?.Uid is not null)
        {
            resource = resourceMaterializer.Materialize(kubernetesObject);
            resourceId = new Uid(kubernetesObject.Metadata.Uid);
        }

        try
        {
            KubernetesEvent<TResource, TKey> kubernetesEvent = new(
                Scope: scope,
                Location: GetLocation(scope, kubernetesObject, malformed),
                PipelineEventType: eventType,
                Resource: resource,
                ResourceId: resourceId,
                Failure: failure,
                Malformed: malformed
            );

            await backgroundQueue.QueueBackgroundWorkItemAsync(kubernetesEvent, ctx);
        }
        catch (Exception ex)
        {
            logger.LogError(
                ex,
                "Watcher for {kubernetesObjectType} and scope {scope} could not enqueue {watcherEventType} - {exceptionMessage}",
                typeof(TKubernetesObject).Name,
                scope,
                eventType,
                ex.Message
            );
        }
    }

    // where the resource lives: the namespace of the object (none for cluster scoped kinds);
    // events without an object (Initialized, Flushed, Error, Throttled) are located at the scope itself
    private static ResourceLocation GetLocation(
        WatcherScope scope,
        TKubernetesObject? kubernetesObject,
        MalformedObject? malformed)
    {
        if (kubernetesObject is not null)
        {
            return new ResourceLocation(scope.ContextName, new NamespaceName(kubernetesObject.Metadata?.NamespaceProperty));
        }

        if (malformed is not null)
        {
            return new ResourceLocation(scope.ContextName, new NamespaceName(malformed.Namespace));
        }

        return scope.ToResourceLocation();
    }
}
