using Microsoft.Extensions.Options;
using System.Threading.Channels;
using TrivyOperator.Dashboard.Infrastructure.BackgroundQueues.Abstractions;

namespace TrivyOperator.Dashboard.Infrastructure.BackgroundQueues;

public class BackgroundQueue<TObject> : IBackgroundQueue<TObject>
    where TObject : class
{
    protected readonly ILogger<BackgroundQueue<TObject>> Logger;
    private Channel<TObject>? queue;
    private readonly BoundedChannelOptions boundedChannelOptions;

    public BackgroundQueue(IOptions<BackgroundQueueOptions> options, ILogger<BackgroundQueue<TObject>> logger)
    {
        Logger = logger;
        boundedChannelOptions = new BoundedChannelOptions(options.Value.Capacity)
        {
            FullMode = BoundedChannelFullMode.Wait,
            SingleWriter = false,
            SingleReader = true,
        };
        logger.LogDebug("Started BackgroundQueue for {objectType}.", typeof(TObject).Name);
    }

    public void StartQueue()
    {
        queue ??= Channel.CreateBounded<TObject>(boundedChannelOptions);
    }

    public async ValueTask QueueBackgroundWorkItemAsync(TObject enqueuedObject, CancellationToken ctx = default)
    {
        ArgumentNullException.ThrowIfNull(enqueuedObject);
        ArgumentNullException.ThrowIfNull(queue);
        LogQueue(enqueuedObject);

        try
        {
            await queue.Writer.WriteAsync(enqueuedObject, ctx);
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

    public async ValueTask<TObject?> DequeueAsync(CancellationToken ctx = default)
    {
        ArgumentNullException.ThrowIfNull(queue);
        try
        {
            TObject dequeuedObject = await queue.Reader.ReadAsync(ctx);
            LogDequeue(dequeuedObject);

            return dequeuedObject;
        }
        catch (OperationCanceledException)
        {
            Logger.LogDebug("Dequeue was cancelled for {objectType}", typeof(TObject).Name);
            return null;
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
