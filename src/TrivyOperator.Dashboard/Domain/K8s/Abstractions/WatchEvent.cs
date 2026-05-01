using k8s;

namespace TrivyOperator.Dashboard.Domain.K8s.Abstractions;

public record WatchEvent<TKubernetesObject>
{
    public WatchEventType Type { get; init; }
    public required TKubernetesObject Object { get; init; }
}
