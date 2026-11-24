using TrivyOperator.Dashboard.Application.Shared.Models;
using TrivyOperator.Dashboard.Domain.Kubernetes.ValueObjects;
using TrivyOperator.Dashboard.Domain.Shared.Abstractions;

namespace TrivyOperator.Dashboard.Application.Kubernetes.Models;

// Scope: the watcher session that emitted the event (watcher state, alerts, lifecycle)
// Location: where the resource lives (stores, per namespace data); for events without a resource, the scope's own
// Failure is set on Error events, Malformed on Skipped events
public record KubernetesEvent<TResource, TKey>(
    WatcherScope Scope,
    ResourceLocation Location,
    PipelineEventType PipelineEventType,
    TResource? Resource,
    Uid? ResourceId,
    WatchFailure? Failure = null,
    MalformedObject? Malformed = null
) where TResource : IEntity<TKey>;
