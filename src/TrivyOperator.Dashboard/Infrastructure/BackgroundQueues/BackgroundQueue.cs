using Microsoft.Extensions.Options;
using System.Threading.Channels;
using TrivyOperator.Dashboard.Application.Shared.BackgroundQueues.Abstractions;

namespace TrivyOperator.Dashboard.Infrastructure.BackgroundQueues;

public class BackgroundQueue<TObject> : IBackgroundQueue<TObject>
    where TObject : class
{
    private readonly ILogger<BackgroundQueue<TObject>> logger;
    private Channel<TObject>? queue;
    private readonly BoundedChannelOptions boundedChannelOptions;

    public BackgroundQueue(IOptions<BackgroundQueueOptions> options, ILogger<BackgroundQueue<TObject>> logger)
    {
        this.logger = logger;
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
        LogQueue();

        try
        {
            await queue.Writer.WriteAsync(enqueuedObject, ctx);
        }
        catch (OperationCanceledException)
        {
            logger.LogDebug("Queueing was cancelled for {objectType}", typeof(TObject).Name);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Could not enqueue {objectType}", typeof(TObject).Name);
        }
    }

    public async ValueTask<TObject?> DequeueAsync(CancellationToken ctx = default)
    {
        ArgumentNullException.ThrowIfNull(queue);
        try
        {
            TObject dequeuedObject = await queue.Reader.ReadAsync(ctx);
            LogDequeue();

            return dequeuedObject;
        }
        catch (OperationCanceledException)
        {
            logger.LogDebug("Dequeue was cancelled for {objectType}", typeof(TObject).Name);
            return null;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Could not dequeue {objectType}", typeof(TObject).Name);
        }

        return null;
    }

    private void LogQueue() => logger.LogDebug(
        "Queueing {objectType}",
        typeof(TObject).Name
    );

    private void LogDequeue() => logger.LogDebug(
        "Dequeued {objectType}",
        typeof(TObject).Name
    );
}
