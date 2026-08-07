using k8s;
using TrivyOperator.Dashboard.Application.K8s.Pipeline;
using TrivyOperator.Dashboard.Domain.K8s.ValueObjects;

namespace TrivyOperator.Dashboard.Application.K8s.Services.WatcherEvents;

public class WatcherEvent<TKubernetesObject> : IWatcherEvent<TKubernetesObject>
    where TKubernetesObject : IKubernetesObject, new()
{
    public WatcherEventType WatcherEventType { get; init; }
    public TKubernetesObject? KubernetesObject { get; init; }
    public string WatcherKey { get; init; } = string.Empty;
    public ContextName ContextName { get; init; } = new();
    public Exception? Exception { get; init; } = null;
    public bool IsStatic { get; init; }
}
