using TrivyOperator.Dashboard.Application.Kubernetes.Models;
using TrivyOperator.Dashboard.Domain.Shared.Abstractions;
using TrivyOperator.Dashboard.Infrastructure.Kubernetes.EventPipeline.Services.BackgroundQueues.Abstractions;
using TrivyOperator.Dashboard.Infrastructure.Kubernetes.EventPipeline.Services.EventDispatchers.Abstractions;
using TrivyOperator.Dashboard.Infrastructure.Kubernetes.EventPipeline.Services.EventProcessors.Abstractions;

namespace TrivyOperator.Dashboard.Infrastructure.Kubernetes.EventPipeline.Services.EventDispatchers;

public class EventPipelineDispatcher<TResource, TKey>(
    IEnumerable<IKubernetesEventProcessor<TResource, TKey>> services,
    IEventPipelineBackgroundQueue<TResource, TKey> backgroundQueue,
    ILogger<EventPipelineDispatcher<TResource, TKey>> logger
) : IEventPipelineDispatcher<TResource, TKey>
    where TResource : class, IEntity<TKey>
{
    private Task? dispatcherQueueProcessor;
    private bool IsQueueProcessingStarted => !dispatcherQueueProcessor?.IsCanceled ?? false;

    public void StartEventsProcessing(CancellationToken ctx = default)
    {
        if (IsQueueProcessingStarted)
        {
            logger.LogWarning(
                "Kubernetes Event Dispatcher for {kubernetesObjectType} already started. Ignoring start request...",
                typeof(TResource).Name
            );
            return;
        }

        logger.LogInformation(
            "KubernetesEventDispatcher for {kubernetesObjectType} is starting.",
            typeof(TResource).Name
        );
        dispatcherQueueProcessor = ProcessChannelMessages(ctx);
    }

    private async Task ProcessChannelMessages(CancellationToken ctx = default)
    {
        while (!ctx.IsCancellationRequested)
        {
            try
            {
                WatcherEvent<TResource, TKey>? watcherEvent = await backgroundQueue.DequeueAsync(ctx);

                if (watcherEvent is null)
                {
                    if (!ctx.IsCancellationRequested)
                    {
                        logger.LogWarning("Received null watcher event. Ignoring...");
                    }

                    continue;
                }

                if (ctx.IsCancellationRequested)
                {
                    break;
                }

                try
                {
                    IEnumerable<Task> tasks = services.Select(service =>
                        service.ProcessKubernetesEvent(watcherEvent, ctx)
                    );
                    await Task.WhenAll(tasks);
                }
                catch (Exception ex)
                {
                    if (ex is AggregateException aggEx)
                    {
                        foreach (Exception inner in aggEx.InnerExceptions)
                        {
                            logger.LogError(
                                inner,
                                "An error occurred while processing the watcher event for {kubernetesObjectType}.",
                                typeof(TResource).Name
                            );
                        }
                    }
                    else
                    {
                        logger.LogError(
                            ex,
                            "An error occurred while processing the watcher event for {kubernetesObjectType}.",
                            typeof(TResource).Name
                        );
                    }
                }
            }
            catch (Exception ex)
            {
                logger.LogError(
                    ex,
                    "Error processing event for {kubernetesObjectType}.",
                    typeof(TResource).Name
                );
            }
        }
    }
}
