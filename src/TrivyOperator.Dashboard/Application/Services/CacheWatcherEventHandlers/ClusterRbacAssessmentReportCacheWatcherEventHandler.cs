﻿using TrivyOperator.Dashboard.Application.Services.BackgroundQueues.Abstractions;
using TrivyOperator.Dashboard.Application.Services.CacheRefresh.Abstractions;
using TrivyOperator.Dashboard.Application.Services.CacheWatcherEventHandlers.Abstractions;
using TrivyOperator.Dashboard.Application.Services.WatcherEvents.Abstractions;
using TrivyOperator.Dashboard.Application.Services.Watchers.Abstractions;
using TrivyOperator.Dashboard.Domain.Trivy.ClusterRbacAssessmentReport;
using TrivyOperator.Dashboard.Domain.Trivy.CustomResources.Abstractions;

namespace TrivyOperator.Dashboard.Application.Services.CacheWatcherEventHandlers;

public class ClusterRbacAssessmentReportWatcherCacheSomething(
    ICacheRefresh<ClusterRbacAssessmentReportCr, IBackgroundQueue<ClusterRbacAssessmentReportCr>> cacheRefresh,
    IClusterScopedWatcher<ClusterRbacAssessmentReportCr> kubernetesWatcher,
    ILogger<ClusterRbacAssessmentReportWatcherCacheSomething> logger)
    : ClusterScopedCacheWatcherEventHandler<IBackgroundQueue<ClusterRbacAssessmentReportCr>,
        ICacheRefresh<ClusterRbacAssessmentReportCr, IBackgroundQueue<ClusterRbacAssessmentReportCr>>,
        WatcherEvent<ClusterRbacAssessmentReportCr>, IClusterScopedWatcher<ClusterRbacAssessmentReportCr>,
        ClusterRbacAssessmentReportCr, CustomResourceList<ClusterRbacAssessmentReportCr>>(
        cacheRefresh,
        kubernetesWatcher,
        logger);
