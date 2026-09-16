using k8s;
using k8s.Models;
using TrivyOperator.Dashboard.Application.KubernetesEventPipeline.Models.WatcherEvents;

namespace TrivyOperator.Dashboard.Application.KubernetesEventPipeline.Services.WatchSessions.Abstractions;

public interface IKubernetesWatchSessionFactory<TKubernetesObjectList, TKubernetesObject>
    where TKubernetesObject : class, IKubernetesObject<V1ObjectMeta>, new()
    where TKubernetesObjectList : IKubernetesObject<V1ListMeta>, IItems<TKubernetesObject>
{
    KubernetesWatchSession<TKubernetesObjectList, TKubernetesObject> Create(WatcherKey key);
}
