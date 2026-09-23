using TrivyOperator.Dashboard.Domain.Shared.Abstractions;

namespace TrivyOperator.Dashboard.Application.Kubernetes.EventPipeline.EventDispatchers.Abstractions;

public interface IEventPipelineDispatcher<TResource, TKey>
    where TResource : class, IEntity<TKey>
{
    void StartEventsProcessing(CancellationToken ctx = default);
}
