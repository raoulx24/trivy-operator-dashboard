using k8s;
using k8s.Models;
using TrivyOperator.Dashboard.Application.K8s.Services.EventCoordinators.Abstractions;
using TrivyOperator.Dashboard.Application.K8s.Services.EventDispatchers.Abstractions;
using TrivyOperator.Dashboard.Application.K8s.Services.Watchers.Abstractions;

namespace TrivyOperator.Dashboard.Application.K8s.Services.EventCoordinators;

public class ClusterScopedKubernetesEventCoordinator<TKubernetesEventDispatcher, TKubernetesWatcher, TKubernetesObject>(
    TKubernetesEventDispatcher kubernetesEventDispatcher,
    TKubernetesWatcher kubernetesWatcher,
    ILogger<ClusterScopedKubernetesEventCoordinator<TKubernetesEventDispatcher, TKubernetesWatcher, TKubernetesObject>>
        logger
) : KubernetesEventCoordinator<TKubernetesEventDispatcher, TKubernetesWatcher, TKubernetesObject>(
    kubernetesEventDispatcher,
    kubernetesWatcher,
    logger
), IClusterScopedKubernetesEventCoordinator
    where TKubernetesEventDispatcher : IKubernetesEventDispatcher<TKubernetesObject>
    where TKubernetesWatcher : IClusterScopedWatcher<TKubernetesObject>
    where TKubernetesObject : class, IKubernetesObject<V1ObjectMeta>;
