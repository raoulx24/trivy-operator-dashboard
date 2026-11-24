using k8s;

namespace TrivyOperator.Dashboard.Domain.Kubernetes.ValueObjects;

// exactly one of Object / Malformed is set
public record WatchEvent<TKubernetesObject>
{
    public WatchEventType Type { get; init; }
    public TKubernetesObject? Object { get; init; }
    public MalformedObject? Malformed { get; init; }
}
