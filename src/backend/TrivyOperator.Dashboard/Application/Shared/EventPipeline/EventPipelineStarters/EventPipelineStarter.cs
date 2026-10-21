using TrivyOperator.Dashboard.Application.Shared.BackgroundQueues.Abstractions;
using TrivyOperator.Dashboard.Application.Shared.EventPipeline.EventDispatchers.Abstractions;
using TrivyOperator.Dashboard.Application.Shared.EventPipeline.EventPipelineStarters.Abstractions;
using TrivyOperator.Dashboard.Application.Types;

namespace TrivyOperator.Dashboard.Application.Shared.EventPipeline.EventPipelineStarters;

public class EventPipelineStarter<TResource>(
    IEventDispatcher<TResource> eventDispatcher,
    IBackgroundQueue<TResource> queue,
    ILogger<EventPipelineStarter<TResource>> logger
) : IEventPipelineStarter
{
    public virtual void StartPipeline(CancellationToken ctx = default)
    {
        logger.LogInformation("Starting Events Pipeline for {resourceType}", typeof(TResource).GetReadableName());
        
        queue.StartQueue();
        eventDispatcher.StartEventsProcessing(ctx);   
    }
}
