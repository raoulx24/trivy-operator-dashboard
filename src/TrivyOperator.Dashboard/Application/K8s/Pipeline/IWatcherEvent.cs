using k8s;
using TrivyOperator.Dashboard.Application.K8s.Services.WatcherEvents;
using TrivyOperator.Dashboard.Domain.K8s.ValueObjects;

namespace TrivyOperator.Dashboard.Application.K8s.Pipeline;

public interface IWatcherEvent<TKubernetesObject>
    where TKubernetesObject : IKubernetesObject
{
    WatcherEventType WatcherEventType { get; init; }
    TKubernetesObject? KubernetesObject { get; init; }
    string WatcherKey { get; init; }
    ContextName ContextName { get; init; }
    Exception? Exception { get; init; }
    bool IsStatic { get; init; }
}
