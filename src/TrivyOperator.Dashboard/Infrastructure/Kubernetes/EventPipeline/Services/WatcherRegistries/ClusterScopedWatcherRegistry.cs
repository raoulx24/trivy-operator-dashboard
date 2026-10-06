using k8s;
using k8s.Models;
using TrivyOperator.Dashboard.Application.Kubernetes.WatcherRegistries.Abstractions;
using TrivyOperator.Dashboard.Domain.Shared.Abstractions;
using TrivyOperator.Dashboard.Infrastructure.Kubernetes.EventPipeline.Services.WatchSessions.Abstractions;

namespace TrivyOperator.Dashboard.Infrastructure.Kubernetes.EventPipeline.Services.WatcherRegistries;

public class ClusterScopedWatcherRegistry<TKubernetesObjectList, TKubernetesObject, TResource, TKey>(
    IKubernetesWatchSessionFactory<TKubernetesObjectList, TKubernetesObject> sessionFactory,
    ILogger<ClusterScopedWatcherRegistry<TKubernetesObjectList, TKubernetesObject, TResource, TKey>> logger
) : KubernetesWatcherRegistry<TKubernetesObjectList, TKubernetesObject>(sessionFactory, logger),
    IClusterScopedWatcherRegistry<TResource, TKey>
    where TKubernetesObject : class, IKubernetesObject<V1ObjectMeta>, new()
    where TKubernetesObjectList : IKubernetesObject<V1ListMeta>, IItems<TKubernetesObject>
    where TResource : class, IEntity<TKey>;
