using k8s;
using TrivyOperator.Dashboard.Domain.Kubernetes.ValueObjects;

namespace TrivyOperator.Dashboard.Infrastructure.Kubernetes.EventPipeline.Models;

public class WatcherEvent<TKubernetesObject>
    where TKubernetesObject : IKubernetesObject, new()
{
    public ResourceLocation Key { get; init; }
    public WatcherEventType WatcherEventType { get; init; }
    public TKubernetesObject? KubernetesObject { get; init; }
    public Exception? Exception { get; init; } = null;
    public bool IsStatic { get; init; } = false;
}
