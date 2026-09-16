using k8s;
using k8s.Models;
using TrivyOperator.Dashboard.Application.KubernetesEventPipeline.Services.WatcherRegistries.Abstractions;
using TrivyOperator.Dashboard.Application.KubernetesEventPipeline.Services.WatchSessions.Abstractions;

namespace TrivyOperator.Dashboard.Application.KubernetesEventPipeline.Services.WatcherRegistries;

public class ClusterScopedWatcherRegistry<TKubernetesObjectList, TKubernetesObject>(
    IKubernetesWatchSessionFactory<TKubernetesObjectList, TKubernetesObject> sessionFactory,
    ILogger<ClusterScopedWatcherRegistry<TKubernetesObjectList, TKubernetesObject>> logger
) 
    : KubernetesWatcherRegistry<TKubernetesObjectList, TKubernetesObject>(sessionFactory, logger),
        IClusterScopedWatcherRegistry
    where TKubernetesObject : class, IKubernetesObject<V1ObjectMeta>, new()
    where TKubernetesObjectList : IKubernetesObject<V1ListMeta>, IItems<TKubernetesObject>
{ }
