﻿using k8s;
using k8s.Models;
using TrivyOperator.Dashboard.Application.Services.BackgroundQueues.Abstractions;
using TrivyOperator.Dashboard.Application.Services.CacheRefresh.Abstractions;
using TrivyOperator.Dashboard.Application.Services.WatcherEvents.Abstractions;
using TrivyOperator.Dashboard.Application.Services.Watchers.Abstractions;

namespace TrivyOperator.Dashboard.Application.Services.CacheWatcherEventHandlers.Abstractions;

public class
    ClusterScopedCacheWatcherEventHandler<TBackgroundQueue, TCacheRefresh, TKubernetesWatcherEvent, TKubernetesWatcher,
        TKubernetesObject,
        TKubernetesObjectList>(
        TCacheRefresh cacheRefresh,
        TKubernetesWatcher kubernetesWatcher,
        ILogger<CacheWatcherEventHandler<TBackgroundQueue, TCacheRefresh, TKubernetesWatcherEvent, TKubernetesWatcher,
            TKubernetesObject, TKubernetesObjectList>> logger)
    : CacheWatcherEventHandler<TBackgroundQueue, TCacheRefresh, TKubernetesWatcherEvent, TKubernetesWatcher,
            TKubernetesObject, TKubernetesObjectList>(cacheRefresh, kubernetesWatcher, logger),
        IClusterScopedCacheWatcherEventHandler where TBackgroundQueue : IBackgroundQueue<TKubernetesObject>
    where TCacheRefresh : ICacheRefresh<TKubernetesObject, TBackgroundQueue>
    where TKubernetesWatcherEvent : class, IWatcherEvent<TKubernetesObject>, new()
    where TKubernetesWatcher : IClusterScopedWatcher<TKubernetesObject>
    where TKubernetesObject : class, IKubernetesObject<V1ObjectMeta>
    where TKubernetesObjectList : IKubernetesObject<V1ListMeta>, IItems<TKubernetesObject>;
