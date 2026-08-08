using k8s.Models;
using System.Collections.Concurrent;
using TrivyOperator.Dashboard.Application.K8s.Models.WatcherEvents;
using TrivyOperator.Dashboard.Application.K8s.Services.EventCoordinators.Abstractions;
using TrivyOperator.Dashboard.Domain.K8s.ValueObjects;
using TrivyOperator.Dashboard.Infrastructure.Caching.InMemoryOld.Abstractions;

namespace TrivyOperator.Dashboard.Application.K8s.Services.CacheRefreshers;

// TODO: extract as dedicated IKubernetesEventProcessor<V1Namespace>

public class NamespaceCacheRefresher(
    IConcurrentDictionaryCache<V1Namespace> cache,
    IEnumerable<INamespacedKubernetesEventCoordinator> services,
    ILogger<NamespaceCacheRefresher> logger
) : CacheRefresher<V1Namespace>(cache, logger)
{
    protected override void ProcessAddEvent(WatcherEvent<V1Namespace> watcherEvent, CancellationToken ctx = default)
    {
        if (watcherEvent.KubernetesObject == null)
        {
            logger.LogWarning(
                "ProcessAddEvent - KubernetesObject is null for {key} - {kubernetesObjectType}. Ignoring",
                watcherEvent.Key,
                nameof(V1Namespace)
            );
            return;
        }

        base.ProcessAddEvent(watcherEvent, ctx);
        if (watcherEvent.WatcherEventType == WatcherEventType.Added ||
            watcherEvent.WatcherEventType == WatcherEventType.InitialAdded)
        {
            foreach (INamespacedKubernetesEventCoordinator service in services)
            {
                service.Start(watcherEvent.Key, ctx);
            }
        }
    }

    protected override void ProcessModifiedEvent(
        WatcherEvent<V1Namespace> watcherEvent,
        CancellationToken ctx = default
    )
    {
        if (watcherEvent.KubernetesObject == null)
        {
            logger.LogWarning(
                "ProcessModifiedEvent - KubernetesObject is null for {key} - {kubernetesObjectType}. Ignoring",
                watcherEvent.Key,
                nameof(V1Namespace)
            );
            return;
        }

        base.ProcessAddEvent(watcherEvent, ctx);
    }

    protected override async Task ProcessDeleteEvent(WatcherEvent<V1Namespace> watcherEvent, CancellationToken ctx = default)
    {
        if (watcherEvent.KubernetesObject == null)
        {
            logger.LogWarning(
                "ProcessDeleteEvent - KubernetesObject is null for {key} - {kubernetesObjectType}. Ignoring",
                watcherEvent.Key,
                nameof(V1Namespace)
            );
            return;
        }

        await base.ProcessDeleteEvent(watcherEvent, ctx);
        IEnumerable<Task> tasks = services.Select(s => s.Stop(watcherEvent.Key, ctx));
        await Task.WhenAll(tasks);
    }

    protected override async Task ProcessInitEvent(WatcherEvent<V1Namespace> watcherEvent, CancellationToken ctx = default)
    {
        if (Cache.TryGetValue(
                watcherEvent.Key.NamespaceName.Value,
                out ConcurrentDictionary<string, V1Namespace>? namespaceNamesCache
            ))
        {
            NamespaceName[] newNamespaceNames = [.. namespaceNamesCache.Select(kvp => new NamespaceName(kvp.Value.Metadata.Name)),];
            IEnumerable<Task> tasks = services.Select(s => s.ReconcileWatchers(watcherEvent.Key.ContextName, newNamespaceNames, ctx));
            await Task.WhenAll(tasks);
        }
    }
}
