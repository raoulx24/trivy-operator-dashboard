using k8s;

namespace TrivyOperator.Dashboard.Domain.Kubernetes.ValueObjects;

public record WatchEvent<TKubernetesObject>
{
    public WatchEventType Type { get; init; }
    public required TKubernetesObject Object { get; init; }
}
