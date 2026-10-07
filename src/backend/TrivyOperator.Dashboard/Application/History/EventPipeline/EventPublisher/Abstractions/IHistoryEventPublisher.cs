using TrivyOperator.Dashboard.Application.Shared.Models;
using TrivyOperator.Dashboard.Domain.Kubernetes.ValueObjects;

namespace TrivyOperator.Dashboard.Application.History.EventPipeline.EventPublisher.Abstractions;

public interface IHistoryEventPublisher<in THistoryResource>
where THistoryResource : class
{
    Task Publish(
        ResourceLocation key,
        PipelineEventType eventType,
        CancellationToken ctx,
        THistoryResource? historyResource = null
    );
}
