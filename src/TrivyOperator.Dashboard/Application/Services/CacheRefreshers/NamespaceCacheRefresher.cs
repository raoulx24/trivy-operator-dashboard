using k8s.Models;
using System.Collections.Concurrent;
using TrivyOperator.Dashboard.Application.Services.KubernetesEventCoordinators.Abstractions;
using TrivyOperator.Dashboard.Application.Services.WatcherEvents;
using TrivyOperator.Dashboard.Application.Services.WatcherEvents.Abstractions;
using TrivyOperator.Dashboard.Infrastructure.Abstractions;
using TrivyOperator.Dashboard.Utils;

namespace TrivyOperator.Dashboard.Application.Services.CacheRefreshers;

public class NamespaceCacheRefresher(
    IConcurrentDictionaryCache<V1Namespace> cache,
    IEnumerable<INamespacedKubernetesEventCoordinator> services,
    ILogger<NamespaceCacheRefresher> logger)
    : CacheRefresher<V1Namespace>(cache, logger)
{
    protected override void ProcessAddEvent(
        IWatcherEvent<V1Namespace> watcherEvent,
        CancellationToken cancellationToken)
    {
        if (watcherEvent.KubernetesObject == null)
        {
            logger.LogWarning("ProcessAddEvent - KubernetesObject is null for {watcherKey} - {kubernetesObjectType}. Ignoring",
                watcherEvent.WatcherKey, nameof(V1Namespace));
            return;
        }

        base.ProcessAddEvent(watcherEvent, cancellationToken);
        if (watcherEvent.WatcherEventType == WatcherEventType.Added || watcherEvent.WatcherEventType == WatcherEventType.InitialAdded)
        {
            foreach (INamespacedKubernetesEventCoordinator service in services)
            {
                service.Start(cancellationToken, watcherEvent.KubernetesObject.Metadata.Name);
            }
        }
    }

    protected override void ProcessModifiedEvent(
        IWatcherEvent<V1Namespace> watcherEvent,
        CancellationToken cancellationToken)
    {
        if (watcherEvent.KubernetesObject == null)
        {
            logger.LogWarning("ProcessModifiedEvent - KubernetesObject is null for {watcherKey} - {kubernetesObjectType}. Ignoring",
                watcherEvent.WatcherKey, nameof(V1Namespace));
            return;
        }
        base.ProcessAddEvent(watcherEvent, cancellationToken);
    }

    protected override async Task ProcessDeleteEvent(IWatcherEvent<V1Namespace> watcherEvent, CancellationToken cancellationToken)
    {
        if (watcherEvent.KubernetesObject == null)
        {
            logger.LogWarning("ProcessDeleteEvent - KubernetesObject is null for {watcherKey} - {kubernetesObjectType}. Ignoring",
                watcherEvent.WatcherKey, nameof(V1Namespace));
            return;
        }

        await base.ProcessDeleteEvent(watcherEvent, cancellationToken);
        IEnumerable<Task> tasks = services.Select(s => s.Stop(cancellationToken, watcherEvent.KubernetesObject.Metadata.Name));
        await Task.WhenAll(tasks);
    }

    protected override async Task ProcessInitEvent(IWatcherEvent<V1Namespace> watcherEvent, CancellationToken cancellationToken)
    {
        if (cache.TryGetValue(CacheUtils.DefaultCacheRefreshKey, out ConcurrentDictionary<string, V1Namespace>? namespaceNamesCache))
        {
            string[] newNamespaceNames = [.. namespaceNamesCache.Select(kvp => kvp.Value.Metadata.Name)];
            IEnumerable<Task> tasks = services.Select(s => s.ReconcileWatchers(newNamespaceNames, cancellationToken));
            await Task.WhenAll(tasks);
        }
    }
}
