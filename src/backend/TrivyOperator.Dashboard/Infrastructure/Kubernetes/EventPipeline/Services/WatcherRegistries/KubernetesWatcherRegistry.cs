using k8s;
using k8s.Models;
using System.Collections.Concurrent;
using TrivyOperator.Dashboard.Application.Kubernetes.WatcherRegistries.Abstractions;
using TrivyOperator.Dashboard.Domain.Kubernetes.ValueObjects;
using TrivyOperator.Dashboard.Infrastructure.Kubernetes.EventPipeline.Services.WatchSessions;
using TrivyOperator.Dashboard.Infrastructure.Kubernetes.EventPipeline.Services.WatchSessions.Abstractions;

namespace TrivyOperator.Dashboard.Infrastructure.Kubernetes.EventPipeline.Services.WatcherRegistries;

// start, stop and recreate of one scope run one at a time (a lock per scope); different scopes run in parallel
// a stop keeps the lock until the session has ended and its Flushed is queued, so the old session's events always
// reach the (single consumer, in order) pipeline before the events of a session started afterwards
// the locks are never removed: one small object per scope ever seen
// sessions live until they are stopped or the application stops; the callers' tokens (an HTTP request, a hosted
// service run) only bound the wait for the lock: once a stop has started, it completes (including its Flushed)
// unless the application is stopping, so a cancelled caller cannot leave a stopped session's data behind
public class KubernetesWatcherRegistry<TKubernetesObjectList, TKubernetesObject>(
    IKubernetesWatchSessionFactory<TKubernetesObjectList, TKubernetesObject> sessionFactory,
    IHostApplicationLifetime applicationLifetime,
    ILogger<KubernetesWatcherRegistry<TKubernetesObjectList, TKubernetesObject>> logger
) : IKubernetesWatcherRegistry
    where TKubernetesObject : class, IKubernetesObject<V1ObjectMeta>, new()
    where TKubernetesObjectList : IKubernetesObject<V1ListMeta>, IItems<TKubernetesObject>
{
    protected readonly ConcurrentDictionary<
        WatcherScope,
        KubernetesWatchSession<TKubernetesObjectList, TKubernetesObject>
    > Sessions = [];

    private readonly ConcurrentDictionary<WatcherScope, SemaphoreSlim> scopeLocks = [];

    public async Task StartWatcher(WatcherScope scope, CancellationToken ctx = default)
    {
        SemaphoreSlim scopeLock = GetScopeLock(scope);
        await scopeLock.WaitAsync(ctx);

        try
        {
            StartWatcherUnlocked(scope);
        }
        finally
        {
            scopeLock.Release();
        }
    }

    public async Task StopWatcher(WatcherScope scope, CancellationToken ctx = default)
    {
        SemaphoreSlim scopeLock = GetScopeLock(scope);
        await scopeLock.WaitAsync(ctx);

        try
        {
            await StopWatcherUnlocked(scope);
        }
        finally
        {
            scopeLock.Release();
        }
    }

    public async Task RecreateWatcher(WatcherScope scope, CancellationToken ctx = default)
    {
        logger.LogWarning(
            "Recreating watcher for {kubernetesObjectType} and scope {scope}.",
            typeof(TKubernetesObject).Name,
            scope
        );

        SemaphoreSlim scopeLock = GetScopeLock(scope);
        await scopeLock.WaitAsync(ctx);

        try
        {
            await StopWatcherUnlocked(scope);
            StartWatcherUnlocked(scope);
        }
        finally
        {
            scopeLock.Release();
        }
    }

    public Type WatchedKubernetesObjectType => typeof(TKubernetesObject);

    private SemaphoreSlim GetScopeLock(WatcherScope scope) =>
        scopeLocks.GetOrAdd(scope, static _ => new SemaphoreSlim(1, 1));

    private void StartWatcherUnlocked(WatcherScope scope)
    {
        if (Sessions.ContainsKey(scope))
        {
            logger.LogWarning(
                "Watcher for {kubernetesObjectType} and scope {scope} already exists. Ignoring start request.",
                typeof(TKubernetesObject).Name,
                scope
            );

            return;
        }

        KubernetesWatchSession<TKubernetesObjectList, TKubernetesObject> session = sessionFactory.Create(scope);

        // only this method adds, under the scope lock, so the add cannot collide
        Sessions[scope] = session;

        logger.LogInformation(
            "Starting watcher for {kubernetesObjectType} and scope {scope}.",
            typeof(TKubernetesObject).Name,
            scope
        );

        session.Start(applicationLifetime.ApplicationStopping);
    }

    private async Task StopWatcherUnlocked(WatcherScope scope)
    {
        logger.LogInformation(
            "Stopping watcher for {kubernetesObjectType} and scope {scope}.",
            typeof(TKubernetesObject).Name,
            scope
        );

        if (!Sessions.TryRemove(scope, out KubernetesWatchSession<TKubernetesObjectList, TKubernetesObject>? session))
        {
            logger.LogWarning(
                "Watcher for {kubernetesObjectType} and scope {scope} not found. Ignoring stop request.",
                typeof(TKubernetesObject).Name,
                scope
            );

            return;
        }

        try
        {
            await session.Stop(applicationLifetime.ApplicationStopping);
        }
        finally
        {
            session.Dispose();
        }
    }
}
