using k8s;
using k8s.Models;
using System.Collections.Concurrent;
using TrivyOperator.Dashboard.Application.KubernetesEventPipeline.Models.WatcherEvents;
using TrivyOperator.Dashboard.Application.KubernetesEventPipeline.Services.WatcherRegistries.Abstractions;
using TrivyOperator.Dashboard.Application.KubernetesEventPipeline.Services.WatchSessions;
using TrivyOperator.Dashboard.Application.KubernetesEventPipeline.Services.WatchSessions.Abstractions;

namespace TrivyOperator.Dashboard.Application.KubernetesEventPipeline.Services.WatcherRegistries;

public sealed class KubernetesWatcherRegistry<TKubernetesObjectList, TKubernetesObject>(
    IKubernetesWatchSessionFactory<TKubernetesObjectList, TKubernetesObject> sessionFactory,
    ILogger<KubernetesWatcherRegistry<TKubernetesObjectList, TKubernetesObject>> logger
) : IKubernetesWatcherRegistry
    where TKubernetesObject : class, IKubernetesObject<V1ObjectMeta>, new()
    where TKubernetesObjectList : IKubernetesObject<V1ListMeta>, IItems<TKubernetesObject>
{
    private readonly ConcurrentDictionary<
        WatcherKey,
        KubernetesWatchSession<TKubernetesObjectList, TKubernetesObject>
    > sessions = [];

    public void StartWatcher(
        WatcherKey key,
        CancellationToken cancellationToken = default
    )
    {
        if (sessions.ContainsKey(key))
        {
            logger.LogWarning(
                "Watcher for {kubernetesObjectType} and key {key} already exists. Ignoring start request.",
                typeof(TKubernetesObject).Name,
                key
            );

            return;
        }

        KubernetesWatchSession<TKubernetesObjectList, TKubernetesObject> session =
            sessionFactory.Create(key);

        if (!sessions.TryAdd(key, session))
        {
            logger.LogWarning(
                "Watcher for {kubernetesObjectType} and key {key} already exists. Ignoring start request.",
                typeof(TKubernetesObject).Name,
                key
            );

            session.Dispose();
            return;
        }

        logger.LogInformation(
            "Starting watcher for {kubernetesObjectType} and key {key}.",
            typeof(TKubernetesObject).Name,
            key
        );

        session.Start(cancellationToken);
    }

    public async Task StopWatcher(
        WatcherKey key,
        CancellationToken cancellationToken = default
    )
    {
        logger.LogInformation(
            "Stopping watcher for {kubernetesObjectType} and key {key}.",
            typeof(TKubernetesObject).Name,
            key
        );

        if (!sessions.TryRemove(key, out KubernetesWatchSession<TKubernetesObjectList, TKubernetesObject>? session))
        {
            logger.LogWarning(
                "Watcher for {kubernetesObjectType} and key {key} not found. Ignoring stop request.",
                typeof(TKubernetesObject).Name,
                key
            );

            return;
        }

        try
        {
            await session.Stop(cancellationToken);
        }
        finally
        {
            session.Dispose();
        }
    }

    public async Task RecreateWatcher(
        WatcherKey key,
        CancellationToken cancellationToken = default
    )
    {
        logger.LogWarning(
            "Recreating watcher for {kubernetesObjectType} and key {key}.",
            typeof(TKubernetesObject).Name,
            key
        );

        await StopWatcher(key, cancellationToken);
        StartWatcher(key, cancellationToken);
    }
    
    public async Task Reconcile(
        IReadOnlyCollection<WatcherKey> desiredKeys,
        CancellationToken cancellationToken = default
    )
    {
        HashSet<WatcherKey> desired = [.. desiredKeys];

        WatcherKey[] existing = [.. sessions.Keys];

        WatcherKey[] toRemove =
        [
            .. existing.Except(desired)
        ];

        WatcherKey[] toAdd =
        [
            .. desired.Except(existing)
        ];

        logger.LogDebug(
            "Reconciling watchers for {kubernetesObjectType}. Existing: {existingCount}, Desired: {desiredCount}, Add: {addCount}, Remove: {removeCount}.",
            typeof(TKubernetesObject).Name,
            existing.Length,
            desired.Count,
            toAdd.Length,
            toRemove.Length
        );

        IEnumerable<Task> stopTasks =
            toRemove.Select(key => StopWatcher(key, cancellationToken));

        foreach (WatcherKey key in toAdd)
        {
            StartWatcher(key, cancellationToken);
        }

        await Task.WhenAll(stopTasks);
    }
}
