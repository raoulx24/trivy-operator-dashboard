namespace TrivyOperator.Dashboard.Application.Shared.BackgroundQueues.Abstractions;

public interface IBackgroundQueue<TObject>
{
    void StartQueue();
    ValueTask<TObject?> DequeueAsync(CancellationToken ctx = default);
    ValueTask QueueBackgroundWorkItemAsync(TObject enqueuedObject, CancellationToken ctx = default);
}
