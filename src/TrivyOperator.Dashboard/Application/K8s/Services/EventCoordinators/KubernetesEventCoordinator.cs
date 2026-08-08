using k8s;
using k8s.Models;
using TrivyOperator.Dashboard.Application.K8s.Models.WatcherEvents;
using TrivyOperator.Dashboard.Application.K8s.Pipeline;
using TrivyOperator.Dashboard.Application.K8s.Services.EventCoordinators.Abstractions;
using TrivyOperator.Dashboard.Application.K8s.Services.Watchers.Abstractions;
using TrivyOperator.Dashboard.Infrastructure.K8s.Contexts.Abstractions;
using TrivyOperator.Dashboard.Infrastructure.Utils;

namespace TrivyOperator.Dashboard.Application.K8s.Services.EventCoordinators;

// TODO: replace TKubernetesEventDispatcher with IKubernetesEventDispatcher<TKubernetesObject>
public class KubernetesEventCoordinator<TKubernetesEventDispatcher, TKubernetesWatcher, TKubernetesObject>(
    TKubernetesEventDispatcher kubernetesEventDispatcher,
    TKubernetesWatcher kubernetesWatcher,
    ILogger<KubernetesEventCoordinator<TKubernetesEventDispatcher, TKubernetesWatcher, TKubernetesObject>> logger
) : IKubernetesEventCoordinator
    where TKubernetesEventDispatcher : IKubernetesEventDispatcher<TKubernetesObject>
    where TKubernetesWatcher : IKubernetesWatcher<TKubernetesObject>
    where TKubernetesObject : class, IKubernetesObject<V1ObjectMeta>
{
    protected readonly TKubernetesEventDispatcher KubernetesEventDispatcher = kubernetesEventDispatcher;
    protected readonly TKubernetesWatcher KubernetesWatcher = kubernetesWatcher;

    protected readonly ILogger<KubernetesEventCoordinator<TKubernetesEventDispatcher, TKubernetesWatcher,
        TKubernetesObject>> Logger = logger;

    public async Task Start(WatcherKey key, CancellationToken ctx)
    {
        Logger.LogDebug(
            "Adding Watcher for {kubernetesObjectType} - {key}.",
            typeof(TKubernetesObject).Name,
            key
        );
        await KubernetesWatcher.Add(key, ctx);
        if (!KubernetesEventDispatcher.IsQueueProcessingStarted)
        {
            Logger.LogDebug("Starting IKubernetesEventDispatcher CacheRefresher for {kubernetesObjectType}.", typeof(TKubernetesObject).Name);
            KubernetesEventDispatcher.StartEventsProcessing(ctx);
        }
    }
}
