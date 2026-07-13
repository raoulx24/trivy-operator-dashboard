namespace TrivyOperator.Dashboard.Infrastructure.BackgroundQueues.Abstractions;

public interface IBackgroundQueue<TObject>
    where TObject : class
{
    ValueTask<TObject?> DequeueAsync(CancellationToken cancellationToken);
    ValueTask QueueBackgroundWorkItemAsync(TObject enqueuedObject, CancellationToken cancellationToken);
}
