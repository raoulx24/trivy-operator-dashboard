using k8s;
using k8s.Models;
using TrivyOperator.Dashboard.Application.K8s.Models.WatcherEvents;
using TrivyOperator.Dashboard.Application.K8s.Pipeline;
using TrivyOperator.Dashboard.Application.K8s.Services.EventCoordinators.Abstractions;
using TrivyOperator.Dashboard.Application.K8s.Services.Watchers.Abstractions;
using TrivyOperator.Dashboard.Domain.K8s.ValueObjects;

namespace TrivyOperator.Dashboard.Application.K8s.Services.EventCoordinators;

public class NamespacedKubernetesEventCoordinator<TKubernetesEventDispatcher, TKubernetesWatcher, TKubernetesObject>(
    TKubernetesEventDispatcher kubernetesEventDispatcher,
    TKubernetesWatcher kubernetesWatcher,
    ILogger<NamespacedKubernetesEventCoordinator<TKubernetesEventDispatcher, TKubernetesWatcher, TKubernetesObject>>
        logger
) : KubernetesEventCoordinator<TKubernetesEventDispatcher, TKubernetesWatcher, TKubernetesObject>(
    kubernetesEventDispatcher,
    kubernetesWatcher,
    logger
), INamespacedKubernetesEventCoordinator
    where TKubernetesEventDispatcher : IKubernetesEventDispatcher<TKubernetesObject>
    where TKubernetesWatcher : INamespacedWatcher<TKubernetesObject>
    where TKubernetesObject : class, IKubernetesObject<V1ObjectMeta>
{
    public async Task Stop(WatcherKey key, CancellationToken ctx = default)
    {
        Logger.LogDebug(
            "Removing Watcher for {kubernetesObjectType} - {key}.",
            typeof(TKubernetesObject).Name,
            key
        );
        await KubernetesWatcher.Delete(key, ctx);
    }

    public async Task ReconcileWatchers(ContextName contextName, IReadOnlyList<NamespaceName> newNamespaceNames, CancellationToken ctx = default) =>
        await KubernetesWatcher.ReconcileNamespaces(contextName, newNamespaceNames, ctx);
}
