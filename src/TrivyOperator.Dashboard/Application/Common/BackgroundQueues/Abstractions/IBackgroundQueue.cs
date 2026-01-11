namespace TrivyOperator.Dashboard.Application.Common.BackgroundQueues.Abstractions;

public interface IBackgroundQueue<TObject> where TObject : class
{
    ValueTask<TObject?> DequeueAsync(CancellationToken cancellationToken);
    ValueTask QueueBackgroundWorkItemAsync(TObject enqueuedObject, CancellationToken cancellationToken);
}