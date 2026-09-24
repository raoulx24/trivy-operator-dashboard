using TrivyOperator.Dashboard.Application.Shared.BackgroundQueues.Abstractions;
using TrivyOperator.Dashboard.Application.Shared.EventPipelineStarters.Abstractions;

namespace TrivyOperator.Dashboard.Application.Shared.EventPipelineStarters;

public class EventPipelineStarter<TObject>(
    IBackgroundQueue<TObject> queue
) : IEventPipelineStarter
{
    public virtual void StartPipeline(CancellationToken ctx = default)
    {
        queue.StartQueue();
    }
}
