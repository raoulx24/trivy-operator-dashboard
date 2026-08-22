using k8s;
using k8s.Models;
using TrivyOperator.Dashboard.Application.K8sEventPipeline.Models.WatcherEvents;
using TrivyOperator.Dashboard.Application.K8sEventPipeline.Services.BackgroundQueues.Abstractions;
using TrivyOperator.Dashboard.Application.K8sEventPipeline.Services.EventDispatchers.Abstractions;
using TrivyOperator.Dashboard.Application.K8sEventPipeline.Services.EventProcessors.Abstractions;

namespace TrivyOperator.Dashboard.Application.K8sEventPipeline.Services.EventDispatchers;

public class KubernetesEventDispatcher<TKubernetesObject, TBackgroundQueue>(
    IEnumerable<IKubernetesEventProcessor<TKubernetesObject>> services,
    TBackgroundQueue backgroundQueue,
    ILogger<KubernetesEventDispatcher<TKubernetesObject, TBackgroundQueue>> logger
) : IKubernetesEventDispatcher<TKubernetesObject>
    where TKubernetesObject : IKubernetesObject<V1ObjectMeta>, new()
    where TBackgroundQueue : IKubernetesBackgroundQueue<TKubernetesObject>
{
    private Task? dispatcherQueueProcessor;
    public bool IsQueueProcessingStarted => !dispatcherQueueProcessor?.IsCanceled ?? false;

    public void StartEventsProcessing(CancellationToken ctx = default)
    {
        if (IsQueueProcessingStarted)
        {
            logger.LogWarning(
                "Kubernetes Event Dispatcher for {kubernetesObjectType} already started. Ignoring start request...",
                typeof(TKubernetesObject).Name
            );
            return;
        }

        logger.LogInformation(
            "KubernetesEventDispatcher for {kubernetesObjectType} is starting.",
            typeof(TKubernetesObject).Name
        );
        dispatcherQueueProcessor = ProcessChannelMessages(ctx);
    }

    private async Task ProcessChannelMessages(CancellationToken ctx = default)
    {
        while (!ctx.IsCancellationRequested)
        {
            try
            {
                WatcherEvent<TKubernetesObject>? watcherEvent = await backgroundQueue.DequeueAsync(ctx);

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
                                typeof(TKubernetesObject).Name
                            );
                        }
                    }
                    else
                    {
                        logger.LogError(
                            ex,
                            "An error occurred while processing the watcher event for {kubernetesObjectType}.",
                            typeof(TKubernetesObject).Name
                        );
                    }
                }
            }
            catch (Exception ex)
            {
                logger.LogError(
                    ex,
                    "Error processing event for {kubernetesObjectType}.",
                    typeof(TKubernetesObject).Name
                );
            }
        }
    }
}
