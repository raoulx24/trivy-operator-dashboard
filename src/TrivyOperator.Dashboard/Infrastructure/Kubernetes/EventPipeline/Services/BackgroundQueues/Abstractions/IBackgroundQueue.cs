namespace TrivyOperator.Dashboard.Infrastructure.Kubernetes.EventPipeline.Services.BackgroundQueues.Abstractions;

public interface IBackgroundQueue<TObject>
    where TObject : class
{
    void StartQueue();
    ValueTask<TObject?> DequeueAsync(CancellationToken ctx = default);
    ValueTask QueueBackgroundWorkItemAsync(TObject enqueuedObject, CancellationToken ctx = default);
}
