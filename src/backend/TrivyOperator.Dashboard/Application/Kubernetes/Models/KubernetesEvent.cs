using TrivyOperator.Dashboard.Application.Shared.Models;
using TrivyOperator.Dashboard.Domain.Kubernetes.ValueObjects;
using TrivyOperator.Dashboard.Domain.Shared.Abstractions;

namespace TrivyOperator.Dashboard.Application.Kubernetes.Models;

public record KubernetesEvent<TResource, TKey>(
    ResourceLocation Key,
    PipelineEventType PipelineEventType,
    TResource? Resource,
    Uid? ResourceId,
    Exception? Exception
) where TResource : IEntity<TKey>;
