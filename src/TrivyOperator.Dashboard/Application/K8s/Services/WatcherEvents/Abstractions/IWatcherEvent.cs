using k8s;

namespace TrivyOperator.Dashboard.Application.K8s.Services.WatcherEvents.Abstractions;

public interface IWatcherEvent<TKubernetesObject>
    where TKubernetesObject : IKubernetesObject
{
    WatcherEventType WatcherEventType { get; init; }
    TKubernetesObject? KubernetesObject { get; init; }
    string WatcherKey { get; init; }
    Exception? Exception { get; init; }
    bool IsStatic { get; init; }
}
