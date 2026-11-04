using TrivyOperator.Dashboard.Application.Kubernetes.EventPipeline.EventProcessors.Abstractions;
using TrivyOperator.Dashboard.Application.Kubernetes.Models;
using TrivyOperator.Dashboard.Domain.Shared.Abstractions;
using TrivyOperator.Dashboard.Domain.Trivy.Entities.Abstracts;

namespace TrivyOperator.Dashboard.Application.Shared.Details.KubernetesEventProcessors;

// hands the slim form of the report to the processor that stores it (ResourceStoreUpdater of the slim type),
// so the full report, with its checks, is not kept
public sealed class SlimProjectionProcessor<TReport, TSlim, TKey>(
    IKubernetesEventProcessor<TSlim, TKey> inner
) : IKubernetesEventProcessor<TReport, TKey>
    where TReport : class, ISlimmable<TSlim>, IEntity<TKey>
    where TSlim : class, IEntity<TKey>
{
    public Task ProcessEvent(KubernetesEvent<TReport, TKey> kubernetesEvent, CancellationToken ctx = default)
    {
        KubernetesEvent<TSlim, TKey> projected = new(
            kubernetesEvent.Key,
            kubernetesEvent.PipelineEventType,
            kubernetesEvent.Resource?.ToSlim(),
            kubernetesEvent.ResourceId,
            kubernetesEvent.Exception);

        return inner.ProcessEvent(projected, ctx);
    }
}
