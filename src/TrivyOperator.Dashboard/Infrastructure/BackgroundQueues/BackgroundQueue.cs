using Microsoft.Extensions.Options;
using System.Threading.Channels;
using TrivyOperator.Dashboard.Infrastructure.BackgroundQueues.Abstractions;

namespace TrivyOperator.Dashboard.Infrastructure.BackgroundQueues;

public class BackgroundQueue<TObject> : IBackgroundQueue<TObject>
    where TObject : class
{
    protected readonly ILogger<BackgroundQueue<TObject>> Logger;
    private readonly Channel<TObject> queue;

    public BackgroundQueue(IOptions<BackgroundQueueOptions> options, ILogger<BackgroundQueue<TObject>> logger)
    {
        this.Logger = logger;
        BoundedChannelOptions boundedChannelOptions = new(options.Value.Capacity)
        {
            FullMode = BoundedChannelFullMode.Wait,
        };
        queue = Channel.CreateBounded<TObject>(boundedChannelOptions);
        logger.LogDebug("Started BackgroundQueue for {objectType}.", typeof(TObject).Name);
    }

    public async ValueTask QueueBackgroundWorkItemAsync(TObject enqueuedObject, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(enqueuedObject);
        LogQueue(enqueuedObject);

        try
        {
            await queue.Writer.WriteAsync(enqueuedObject, cancellationToken);
        }
        catch (OperationCanceledException)
        {
            Logger.LogDebug("Queueing was cancelled for {objectType}", typeof(TObject).Name);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Could not enqueue {objectType}", typeof(TObject).Name);
        }
    }

    public async ValueTask<TObject?> DequeueAsync(CancellationToken cancellationToken)
    {
        try
        {
            TObject dequeuedObject = await queue.Reader.ReadAsync(cancellationToken);
            LogDequeue(dequeuedObject);

            return dequeuedObject;
        }
        catch (OperationCanceledException)
        {
            Logger.LogDebug("Dequeue was cancelled for {objectType}", typeof(TObject).Name);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Could not dequeue {objectType}", typeof(TObject).Name);
        }

        return null;
    }

    protected virtual void LogQueue(TObject enqueuedObject) => Logger.LogDebug(
        "Queueing {objectType}",
        typeof(TObject).Name
    );

    protected virtual void LogDequeue(TObject dequeuedObject) => Logger.LogDebug(
        "Dequeued {objectType}",
        typeof(TObject).Name
    );
}
