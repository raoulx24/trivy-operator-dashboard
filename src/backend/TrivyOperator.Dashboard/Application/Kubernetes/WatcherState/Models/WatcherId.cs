using TrivyOperator.Dashboard.Domain.Kubernetes.ValueObjects;

namespace TrivyOperator.Dashboard.Application.Kubernetes.WatcherState.Models;

public readonly record struct WatcherId(
    Type WatchedKubernetesObjectType,
    ResourceLocation Location);
