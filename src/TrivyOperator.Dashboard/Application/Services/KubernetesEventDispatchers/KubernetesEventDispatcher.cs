using k8s;
using k8s.Models;
using TrivyOperator.Dashboard.Application.Services.BackgroundQueues.Abstractions;
using TrivyOperator.Dashboard.Application.Services.KubernetesEventDispatchers.Abstractions;
using TrivyOperator.Dashboard.Application.Services.WatcherEvents.Abstractions;

namespace TrivyOperator.Dashboard.Application.Services.KubernetesEventDispatchers;

public class KubernetesEventDispatcher<TKubernetesObject, TBackgroundQueue>(
    IEnumerable<IKubernetesEventProcessor<TKubernetesObject>> services,
    TBackgroundQueue backgroundQueue,
    ILogger<KubernetesEventDispatcher<TKubernetesObject, TBackgroundQueue>> logger) : IKubernetesEventDispatcher<TKubernetesObject>
    where TKubernetesObject : IKubernetesObject<V1ObjectMeta>
    where TBackgroundQueue : IKubernetesBackgroundQueue<TKubernetesObject>
{
    protected Task? dispatcherQueueProcessor;
    public bool IsQueueProcessingStarted => !dispatcherQueueProcessor?.IsCanceled ?? false;

    public void StartEventsProcessing(CancellationToken cancellationToken)
    {
        if (IsQueueProcessingStarted)
        {
            logger.LogWarning(
                "Kubernetes Event Dispatcher for {kubernetesObjectType} already started. Ignoring...",
                typeof(TKubernetesObject).Name);
            return;
        }
        logger.LogInformation("KubernetesEventDispatcher for {kubernetesObjectType} is starting.", typeof(TKubernetesObject).Name);
        dispatcherQueueProcessor = ProcessChannelMessages(cancellationToken);
    }

    protected virtual async Task ProcessChannelMessages(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                IWatcherEvent<TKubernetesObject>? watcherEvent = await backgroundQueue.DequeueAsync(cancellationToken);

                if (watcherEvent is null)
                {
                    if(!cancellationToken.IsCancellationRequested)
                    {
                        logger.LogWarning("Received null watcher event. Ignoring...");
                    }
                    continue;
                }
                if (cancellationToken.IsCancellationRequested) break;
                try
                {
                    IEnumerable<Task> tasks = services.Select(service => service.ProcessKubernetesEvent(watcherEvent, cancellationToken));
                    await Task.WhenAll(tasks);
                }
                catch (Exception ex)
                {
                    if (ex is AggregateException aggEx)
                    {
                        foreach (var inner in aggEx.InnerExceptions)
                        {
                            logger.LogError(inner,
                                "An error occurred while processing the watcher event for {kubernetesObjectType}.",
                                typeof(TKubernetesObject).Name);
                        }
                    }
                    else
                    {
                        logger.LogError(ex,
                            "An error occurred while processing the watcher event for {kubernetesObjectType}.",
                            typeof(TKubernetesObject).Name);
                    }
                }
            }
            catch (Exception ex)
            {
                logger.LogError(
                    ex,
                    "Error processing event for {kubernetesObjectType}.",
                    typeof(TKubernetesObject).Name);
            }
        }
    }
}
