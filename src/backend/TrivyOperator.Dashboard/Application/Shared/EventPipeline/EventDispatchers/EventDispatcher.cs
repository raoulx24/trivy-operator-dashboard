using TrivyOperator.Dashboard.Application.Shared.BackgroundQueues.Abstractions;
using TrivyOperator.Dashboard.Application.Shared.EventPipeline.EventDispatchers.Abstractions;
using TrivyOperator.Dashboard.Application.Shared.EventPipeline.EventProcessors.Abstractions;

namespace TrivyOperator.Dashboard.Application.Shared.EventPipeline.EventDispatchers;

public class EventDispatcher<TResource, TEvent>(
    IEnumerable<IEventProcessor<TEvent>> services,
    IBackgroundQueue<TEvent> backgroundQueue,
    ILogger<EventDispatcher<TResource, TEvent>> logger
) : IEventDispatcher<TResource>
{
    private Task? dispatcherQueueProcessor;
    private bool IsQueueProcessingStarted => !dispatcherQueueProcessor?.IsCanceled ?? false;

    public void StartEventsProcessing(CancellationToken ctx = default)
    {
        if (IsQueueProcessingStarted)
        {
            logger.LogWarning(
                "Event Dispatcher for {resourceType} already started. Ignoring start request...",
                typeof(TResource).Name
            );
            return;
        }

        logger.LogInformation(
            "Event Dispatcher for {resourceType} is starting.",
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
                TEvent? pipeEvent = await backgroundQueue.DequeueAsync(ctx);

                if (pipeEvent is null)
                {
                    if (!ctx.IsCancellationRequested)
                    {
                        logger.LogWarning("Received null event. Ignoring...");
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
                        service.ProcessEvent(pipeEvent, ctx)
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
                                "An error occurred while processing the event for {resourceType}.",
                                typeof(TResource).Name
                            );
                        }
                    }
                    else
                    {
                        logger.LogError(
                            ex,
                            "An error occurred while processing the event for {resourceType}.",
                            typeof(TResource).Name
                        );
                    }
                }
            }
            catch (Exception ex)
            {
                logger.LogError(
                    ex,
                    "Error processing event for {resourceType}.",
                    typeof(TResource).Name
                );
            }
        }
    }
}

