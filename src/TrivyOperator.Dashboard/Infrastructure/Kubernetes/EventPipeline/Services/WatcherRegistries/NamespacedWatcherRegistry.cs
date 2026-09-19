using k8s;
using k8s.Models;
using TrivyOperator.Dashboard.Application.Kubernetes.WatcherRegistries.Abstractions;
using TrivyOperator.Dashboard.Domain.Kubernetes.ValueObjects;
using TrivyOperator.Dashboard.Infrastructure.Kubernetes.EventPipeline.Services.WatchSessions.Abstractions;

namespace TrivyOperator.Dashboard.Infrastructure.Kubernetes.EventPipeline.Services.WatcherRegistries;

public class NamespacedWatcherRegistry<TKubernetesObjectList, TKubernetesObject>(
    IKubernetesWatchSessionFactory<TKubernetesObjectList, TKubernetesObject> sessionFactory,
    ILogger<NamespacedWatcherRegistry<TKubernetesObjectList, TKubernetesObject>> logger
    ) 
    : KubernetesWatcherRegistry<TKubernetesObjectList, TKubernetesObject>(sessionFactory, logger),
        INamespacedWatcherRegistry
    where TKubernetesObject : class, IKubernetesObject<V1ObjectMeta>, new()
    where TKubernetesObjectList : IKubernetesObject<V1ListMeta>, IItems<TKubernetesObject>
{
    public async Task Reconcile(IReadOnlyCollection<ResourceLocation> desiredKeys, CancellationToken ctx = default)
    {
        HashSet<ResourceLocation> desired = [.. desiredKeys,];

        ResourceLocation[] existing = [.. Sessions.Keys,];

        ResourceLocation[] toRemove = [.. existing.Except(desired),];

        ResourceLocation[] toAdd = [.. desired.Except(existing),];

        logger.LogDebug(
            "Reconciling watchers for {kubernetesObjectType}. Existing: {existingCount}, Desired: {desiredCount}, Add: {addCount}, Remove: {removeCount}.",
            typeof(TKubernetesObject).Name,
            existing.Length,
            desired.Count,
            toAdd.Length,
            toRemove.Length
        );

        IEnumerable<Task> stopTasks = toRemove.Select(key => StopWatcher(key, ctx));

        foreach (ResourceLocation key in toAdd)
        {
            StartWatcher(key, ctx);
        }

        await Task.WhenAll(stopTasks);
    }
}
