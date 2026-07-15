using k8s;
using TrivyOperator.Dashboard.Application.K8s.Services.WatcherEvents;

namespace TrivyOperator.Dashboard.Application.K8s.Pipeline;

public interface IWatcherEvent<TKubernetesObject>
    where TKubernetesObject : IKubernetesObject
{
    WatcherEventType WatcherEventType { get; init; }
    TKubernetesObject? KubernetesObject { get; init; }
    string WatcherKey { get; init; }
    Exception? Exception { get; init; }
    bool IsStatic { get; init; }
}
