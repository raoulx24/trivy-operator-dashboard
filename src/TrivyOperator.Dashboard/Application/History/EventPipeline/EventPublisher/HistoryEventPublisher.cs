using TrivyOperator.Dashboard.Application.History.EventPipeline.BackgroundQueue;
using TrivyOperator.Dashboard.Application.History.EventPipeline.EventPublisher.Abstractions;
using TrivyOperator.Dashboard.Application.History.VulnerabilityReportsHistory.Models;
using TrivyOperator.Dashboard.Application.Shared.Models;
using TrivyOperator.Dashboard.Domain.Kubernetes.ValueObjects;

namespace TrivyOperator.Dashboard.Application.History.EventPipeline.EventPublisher;

public sealed class HistoryEventPublisher<THistoryResource>(
    IHistoryBackgroundQueue<THistoryResource> backgroundQueue,
    ILogger<HistoryEventPublisher<THistoryResource>> logger
) : IHistoryEventPublisher<THistoryResource>
    where THistoryResource : class
{
    public async Task Publish(
        ResourceLocation key,
        PipelineEventType eventType,
        CancellationToken ctx,
        THistoryResource? resource
    )
    {
        logger.LogDebug(
            "Sending history event to queue - {historyResourceType} - {eventType} - {key}",
            typeof(THistoryResource).Name,
            eventType,
            key
        );

        HistoryEvent<THistoryResource> historyEvent = new(
            Key: key,
            PipelineEventType: eventType,
            Resource: resource
        );

        await backgroundQueue.QueueBackgroundWorkItemAsync(historyEvent, ctx);
    }
}
