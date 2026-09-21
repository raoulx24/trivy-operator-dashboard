using TrivyOperator.Dashboard.Domain.Shared.Abstractions;

namespace TrivyOperator.Dashboard.Infrastructure.Kubernetes.EventPipeline.Services.EventDispatchers.Abstractions;

public interface IEventPipelineDispatcher<TResource, TKey>
    where TResource : class, IEntity<TKey>
{
    void StartEventsProcessing(CancellationToken ctx = default);
}
