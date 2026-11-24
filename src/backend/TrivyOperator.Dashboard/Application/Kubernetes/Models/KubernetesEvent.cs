using TrivyOperator.Dashboard.Application.Shared.Models;
using TrivyOperator.Dashboard.Domain.Kubernetes.ValueObjects;
using TrivyOperator.Dashboard.Domain.Shared.Abstractions;

namespace TrivyOperator.Dashboard.Application.Kubernetes.Models;

// Failure is set on Error events, Malformed on Skipped events
public record KubernetesEvent<TResource, TKey>(
    ResourceLocation Key,
    PipelineEventType PipelineEventType,
    TResource? Resource,
    Uid? ResourceId,
    WatchFailure? Failure = null,
    MalformedObject? Malformed = null
) where TResource : IEntity<TKey>;
