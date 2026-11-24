using k8s;
using k8s.Models;
using TrivyOperator.Dashboard.Application.Kubernetes.WatcherRegistries.Abstractions;
using TrivyOperator.Dashboard.Domain.Kubernetes.ValueObjects;
using TrivyOperator.Dashboard.Infrastructure.Kubernetes.EventPipeline.Services.WatchSessions.Abstractions;

namespace TrivyOperator.Dashboard.Infrastructure.Kubernetes.EventPipeline.Services.WatcherRegistries;

public class NamespacedWatcherRegistry<TKubernetesObjectList, TKubernetesObject>(
    IKubernetesWatchSessionFactory<TKubernetesObjectList, TKubernetesObject> sessionFactory,
    IHostApplicationLifetime applicationLifetime,
    ILogger<NamespacedWatcherRegistry<TKubernetesObjectList, TKubernetesObject>> logger
    )
    : KubernetesWatcherRegistry<TKubernetesObjectList, TKubernetesObject>(sessionFactory, applicationLifetime, logger),
        INamespacedWatcherRegistry
    where TKubernetesObject : class, IKubernetesObject<V1ObjectMeta>, new()
    where TKubernetesObjectList : IKubernetesObject<V1ListMeta>, IItems<TKubernetesObject>
{
    public async Task Reconcile(IReadOnlyCollection<WatcherScope> desiredScopes, CancellationToken ctx = default)
    {
        HashSet<WatcherScope> desired = [.. desiredScopes,];

        WatcherScope[] existing = [.. Sessions.Keys,];

        WatcherScope[] toRemove = [.. existing.Except(desired),];

        WatcherScope[] toAdd = [.. desired.Except(existing),];

        logger.LogDebug(
            "Reconciling watchers for {kubernetesObjectType}. Existing: {existingCount}, Desired: {desiredCount}, Add: {addCount}, Remove: {removeCount}.",
            typeof(TKubernetesObject).Name,
            existing.Length,
            desired.Count,
            toAdd.Length,
            toRemove.Length
        );

        // different scopes, so stops and starts do not wait for each other
        Task[] tasks =
        [
            .. toRemove.Select(scope => StopWatcher(scope, ctx)),
            .. toAdd.Select(scope => StartWatcher(scope, ctx)),
        ];

        await Task.WhenAll(tasks);
    }
}
