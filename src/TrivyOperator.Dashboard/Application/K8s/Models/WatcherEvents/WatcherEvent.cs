using k8s;
using TrivyOperator.Dashboard.Domain.K8s.ValueObjects;

namespace TrivyOperator.Dashboard.Application.K8s.Models.WatcherEvents;

public class WatcherEvent<TKubernetesObject>
    where TKubernetesObject : IKubernetesObject, new()
{
    public WatcherKey Key { get; init; }
    public WatcherEventType WatcherEventType { get; init; }
    public TKubernetesObject? KubernetesObject { get; init; }
    public Exception? Exception { get; init; } = null;
    public bool IsStatic { get; init; } = false;
}
