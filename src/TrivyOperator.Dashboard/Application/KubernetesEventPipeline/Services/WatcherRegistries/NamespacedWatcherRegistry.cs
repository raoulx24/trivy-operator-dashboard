using k8s;
using k8s.Models;
using TrivyOperator.Dashboard.Application.KubernetesEventPipeline.Models.WatcherEvents;
using TrivyOperator.Dashboard.Application.KubernetesEventPipeline.Services.WatcherRegistries.Abstractions;
using TrivyOperator.Dashboard.Application.KubernetesEventPipeline.Services.WatchSessions.Abstractions;

namespace TrivyOperator.Dashboard.Application.KubernetesEventPipeline.Services.WatcherRegistries;

public class NamespacedWatcherRegistry<TKubernetesObjectList, TKubernetesObject>(
    IKubernetesWatchSessionFactory<TKubernetesObjectList, TKubernetesObject> sessionFactory,
    ILogger<NamespacedWatcherRegistry<TKubernetesObjectList, TKubernetesObject>> logger
    ) 
    : KubernetesWatcherRegistry<TKubernetesObjectList, TKubernetesObject>(sessionFactory, logger),
        INamespacedWatcherRegistry
    where TKubernetesObject : class, IKubernetesObject<V1ObjectMeta>, new()
    where TKubernetesObjectList : IKubernetesObject<V1ListMeta>, IItems<TKubernetesObject>
{
    public async Task Reconcile(IReadOnlyCollection<WatcherKey> desiredKeys, CancellationToken ctx = default)
    {
        HashSet<WatcherKey> desired = [.. desiredKeys,];

        WatcherKey[] existing = [.. Sessions.Keys,];

        WatcherKey[] toRemove = [.. existing.Except(desired),];

        WatcherKey[] toAdd = [.. desired.Except(existing),];

        logger.LogDebug(
            "Reconciling watchers for {kubernetesObjectType}. Existing: {existingCount}, Desired: {desiredCount}, Add: {addCount}, Remove: {removeCount}.",
            typeof(TKubernetesObject).Name,
            existing.Length,
            desired.Count,
            toAdd.Length,
            toRemove.Length
        );

        IEnumerable<Task> stopTasks = toRemove.Select(key => StopWatcher(key, ctx));

        foreach (WatcherKey key in toAdd)
        {
            StartWatcher(key, ctx);
        }

        await Task.WhenAll(stopTasks);
    }
}
