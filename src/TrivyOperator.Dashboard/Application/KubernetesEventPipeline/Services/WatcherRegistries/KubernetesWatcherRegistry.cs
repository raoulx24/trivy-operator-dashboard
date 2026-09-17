using k8s;
using k8s.Models;
using System.Collections.Concurrent;
using TrivyOperator.Dashboard.Application.KubernetesEventPipeline.Models.WatcherEvents;
using TrivyOperator.Dashboard.Application.KubernetesEventPipeline.Services.WatcherRegistries.Abstractions;
using TrivyOperator.Dashboard.Application.KubernetesEventPipeline.Services.WatchSessions;
using TrivyOperator.Dashboard.Application.KubernetesEventPipeline.Services.WatchSessions.Abstractions;

namespace TrivyOperator.Dashboard.Application.KubernetesEventPipeline.Services.WatcherRegistries;

public class KubernetesWatcherRegistry<TKubernetesObjectList, TKubernetesObject>(
    IKubernetesWatchSessionFactory<TKubernetesObjectList, TKubernetesObject> sessionFactory,
    ILogger<KubernetesWatcherRegistry<TKubernetesObjectList, TKubernetesObject>> logger
) : IKubernetesWatcherRegistry
    where TKubernetesObject : class, IKubernetesObject<V1ObjectMeta>, new()
    where TKubernetesObjectList : IKubernetesObject<V1ListMeta>, IItems<TKubernetesObject>
{
    protected readonly ConcurrentDictionary<
        WatcherKey,
        KubernetesWatchSession<TKubernetesObjectList, TKubernetesObject>
    > Sessions = [];
    
    public void StartWatcher(WatcherKey key, CancellationToken ctx = default)
    {
        if (Sessions.ContainsKey(key))
        {
            logger.LogWarning(
                "Watcher for {kubernetesObjectType} and key {key} already exists. Ignoring start request.",
                typeof(TKubernetesObject).Name,
                key
            );

            return;
        }

        KubernetesWatchSession<TKubernetesObjectList, TKubernetesObject> session = sessionFactory.Create(key);

        if (!Sessions.TryAdd(key, session))
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

        session.Start(ctx);
    }

    public async Task StopWatcher(WatcherKey key, CancellationToken ctx = default)
    {
        logger.LogInformation(
            "Stopping watcher for {kubernetesObjectType} and key {key}.",
            typeof(TKubernetesObject).Name,
            key
        );

        if (!Sessions.TryRemove(key, out KubernetesWatchSession<TKubernetesObjectList, TKubernetesObject>? session))
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
            await session.Stop(ctx);
        }
        finally
        {
            session.Dispose();
        }
    }

    public async Task RecreateWatcher(WatcherKey key, CancellationToken ctx = default)
    {
        logger.LogWarning(
            "Recreating watcher for {kubernetesObjectType} and key {key}.",
            typeof(TKubernetesObject).Name,
            key
        );

        await StopWatcher(key, ctx);
        StartWatcher(key, ctx);
    }

    public Type WatchedKubernetesObjectType => typeof(TKubernetesObject);
}
