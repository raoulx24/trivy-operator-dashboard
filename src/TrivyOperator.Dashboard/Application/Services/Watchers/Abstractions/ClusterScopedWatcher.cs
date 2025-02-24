﻿using k8s;
using k8s.Autorest;
using k8s.Models;
using TrivyOperator.Dashboard.Application.Services.BackgroundQueues.Abstractions;
using TrivyOperator.Dashboard.Application.Services.WatcherEvents.Abstractions;
using TrivyOperator.Dashboard.Application.Services.WatcherStates;
using TrivyOperator.Dashboard.Domain.Services.Abstractions;
using TrivyOperator.Dashboard.Utils;

namespace TrivyOperator.Dashboard.Application.Services.Watchers.Abstractions;

public class ClusterScopedWatcher<TKubernetesObjectList, TKubernetesObject, TBackgroundQueue, TKubernetesWatcherEvent>(
    IClusterScopedResourceWatchDomainService<TKubernetesObject, TKubernetesObjectList>
        clusterScopResourceWatchDomainService,
    TBackgroundQueue backgroundQueue,
    IBackgroundQueue<WatcherStateInfo> backgroundQueueWatcherState,
    ILogger<ClusterScopedWatcher<TKubernetesObjectList, TKubernetesObject, TBackgroundQueue, TKubernetesWatcherEvent>>
        logger)
    : KubernetesWatcher<TKubernetesObjectList, TKubernetesObject, TBackgroundQueue, TKubernetesWatcherEvent>(
        backgroundQueue,
        backgroundQueueWatcherState,
        logger), IClusterScopedWatcher<TKubernetesObject>
    where TKubernetesObject : class, IKubernetesObject<V1ObjectMeta>, new()
    where TKubernetesObjectList : IKubernetesObject<V1ListMeta>, IItems<TKubernetesObject>
    where TKubernetesWatcherEvent : IWatcherEvent<TKubernetesObject>, new()
    where TBackgroundQueue : IKubernetesBackgroundQueue<TKubernetesObject>
{
    protected override Task<HttpOperationResponse<TKubernetesObjectList>> GetKubernetesObjectWatchList(
        string watcherKey,
        string? lastResourceVersion,
        CancellationToken? cancellationToken = null) => clusterScopResourceWatchDomainService.GetResourceWatchList(
        lastResourceVersion,
        GetWatcherRandomTimeout(),
        cancellationToken);

    protected override async Task EnqueueWatcherEventWithError(string watcherKey = VarUtils.DefaultCacheRefreshKey)
    {
        TKubernetesObject kubernetesObject = new();
        WatcherEvent<TKubernetesObject> watcherEvent =
            new() { KubernetesObject = kubernetesObject, WatcherEventType = WatchEventType.Error };

        await BackgroundQueue.QueueBackgroundWorkItemAsync(watcherEvent);
    }

    protected override async Task<TKubernetesObjectList> GetInitialResources(
        string watcherKey,
        string? continueToken,
        CancellationToken? cancellationToken) => await clusterScopResourceWatchDomainService.GetResourceList(
        resourceListPageSize,
        continueToken,
        cancellationToken);
}
