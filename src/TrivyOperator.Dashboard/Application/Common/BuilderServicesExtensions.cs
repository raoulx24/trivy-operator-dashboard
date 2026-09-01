using k8s;
using k8s.Models;
using OpenTelemetry.Exporter;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using System.Reflection;
using TrivyOperator.Dashboard.Api.HealthChecks;
using TrivyOperator.Dashboard.Application.Alerts.Abstractions;
using TrivyOperator.Dashboard.Application.Alerts.Models;
using TrivyOperator.Dashboard.Application.Alerts.Refresher;
using TrivyOperator.Dashboard.Application.GitHub.Options;
using TrivyOperator.Dashboard.Application.GitHub.Services;
using TrivyOperator.Dashboard.Application.History.VulnerabilityReportsHistory.Retention;
using TrivyOperator.Dashboard.Application.History.VulnerabilityReportsHistory.Services;
using TrivyOperator.Dashboard.Application.K8s.Services;
using TrivyOperator.Dashboard.Application.K8s.Services.CacheRefreshers;
using TrivyOperator.Dashboard.Application.K8s.Services.EventCoordinators.Abstractions;
using TrivyOperator.Dashboard.Application.K8s.Services.RawDomain;
using TrivyOperator.Dashboard.Application.K8s.Services.RawDomain.Abstracts;
using TrivyOperator.Dashboard.Application.K8s.Services.WatcherStates;
using TrivyOperator.Dashboard.Application.K8sEventPipeline.HostedServices;
using TrivyOperator.Dashboard.Application.K8sEventPipeline.Models.WatcherEvents;
using TrivyOperator.Dashboard.Application.K8sEventPipeline.Services.BackgroundQueues;
using TrivyOperator.Dashboard.Application.K8sEventPipeline.Services.BackgroundQueues.Abstractions;
using TrivyOperator.Dashboard.Application.K8sEventPipeline.Services.EventDispatchers;
using TrivyOperator.Dashboard.Application.K8sEventPipeline.Services.EventDispatchers.Abstractions;
using TrivyOperator.Dashboard.Application.K8sEventPipeline.Services.EventProcessors.Abstractions;
using TrivyOperator.Dashboard.Application.K8sEventPipeline.Services.Options;
using TrivyOperator.Dashboard.Application.Queries.Alerts.Models;
using TrivyOperator.Dashboard.Application.Queries.Alerts.Services;
using TrivyOperator.Dashboard.Application.Queries.Alerts.Services.Abstractions;
using TrivyOperator.Dashboard.Application.Queries.AppVersions.Services;
using TrivyOperator.Dashboard.Application.Queries.AppVersions.Services.Abstractions;
using TrivyOperator.Dashboard.Application.Queries.BackendSettings.Services;
using TrivyOperator.Dashboard.Application.Queries.BackendSettings.Services.Abstractions;
using TrivyOperator.Dashboard.Application.Queries.Contexts;
using TrivyOperator.Dashboard.Application.Queries.Contexts.Abstractions;
using TrivyOperator.Dashboard.Application.Queries.History.Services;
using TrivyOperator.Dashboard.Application.Queries.History.Services.Abstractions;
using TrivyOperator.Dashboard.Application.Queries.Namespaces.Services;
using TrivyOperator.Dashboard.Application.Queries.Namespaces.Services.Abstractions;
using TrivyOperator.Dashboard.Application.Queries.Trivy.Options;
using TrivyOperator.Dashboard.Application.Queries.Trivy.Services.ClusterVulnerabilityReports;
using TrivyOperator.Dashboard.Application.Queries.Trivy.Services.ClusterVulnerabilityReports.Abstractions;
using TrivyOperator.Dashboard.Application.Queries.Trivy.Services.ExposedSecretReports;
using TrivyOperator.Dashboard.Application.Queries.Trivy.Services.SbomReports;
using TrivyOperator.Dashboard.Application.Queries.Trivy.Services.SbomReports.Abstractions;
using TrivyOperator.Dashboard.Application.Queries.Trivy.Services.VulnerabilityReports;
using TrivyOperator.Dashboard.Application.Queries.Trivy.Services.VulnerabilityReports.Abstractions;
using TrivyOperator.Dashboard.Application.Queries.WatcherStates.Services;
using TrivyOperator.Dashboard.Application.Queries.WatcherStates.Services.Abstractions;
using TrivyOperator.Dashboard.Application.Trivy.Services.ClusterComplianceReport;
using TrivyOperator.Dashboard.Application.Trivy.Services.ClusterComplianceReport.Abstractions;
using TrivyOperator.Dashboard.Application.Trivy.Services.ClusterInfraAssessmentReport;
using TrivyOperator.Dashboard.Application.Trivy.Services.ClusterInfraAssessmentReport.Abstractions;
using TrivyOperator.Dashboard.Application.Trivy.Services.ClusterRbacAssessmentReport;
using TrivyOperator.Dashboard.Application.Trivy.Services.ClusterRbacAssessmentReport.Abstractions;
using TrivyOperator.Dashboard.Application.Trivy.Services.ClusterSbomReport;
using TrivyOperator.Dashboard.Application.Trivy.Services.ClusterSbomReport.Abstractions;
using TrivyOperator.Dashboard.Application.Trivy.Services.ConfigAuditReport;
using TrivyOperator.Dashboard.Application.Trivy.Services.ConfigAuditReport.Abstractions;
using TrivyOperator.Dashboard.Application.Trivy.Services.ExposedSecretReport.Abstractions;
using TrivyOperator.Dashboard.Application.Trivy.Services.InfraAssessmentReport;
using TrivyOperator.Dashboard.Application.Trivy.Services.InfraAssessmentReport.Abstractions;
using TrivyOperator.Dashboard.Application.Trivy.Services.RbacAssessmentReport;
using TrivyOperator.Dashboard.Application.Trivy.Services.RbacAssessmentReport.Abstractions;
using TrivyOperator.Dashboard.Application.Trivy.Services.TrivyReportDependencies;
using TrivyOperator.Dashboard.Application.Trivy.Services.TrivyReportDependencies.Abstractions;
using TrivyOperator.Dashboard.Application.Trivy.Services.VulnerabilityReport;
using TrivyOperator.Dashboard.Application.WatcherStates.HostedServices;
using TrivyOperator.Dashboard.Application.WatcherStates.Models;
using TrivyOperator.Dashboard.Application.WatcherStates.Services;
using TrivyOperator.Dashboard.Domain.History.VulnerabilityReportsHistory;
using TrivyOperator.Dashboard.Domain.History.VulnerabilityReportsHistory.Services;
using TrivyOperator.Dashboard.Domain.History.VulnerabilityReportsHistory.Services.Abstractions;
using TrivyOperator.Dashboard.Domain.History.VulnerabilityReportsHistory.Stores.Abstractions;
using TrivyOperator.Dashboard.Domain.TrivyOld.ClusterComplianceReport;
using TrivyOperator.Dashboard.Domain.TrivyOld.ClusterInfraAssessmentReport;
using TrivyOperator.Dashboard.Domain.TrivyOld.ClusterRbacAssessmentReport;
using TrivyOperator.Dashboard.Domain.TrivyOld.ClusterSbomReport;
using TrivyOperator.Dashboard.Domain.TrivyOld.ClusterVulnerabilityReport;
using TrivyOperator.Dashboard.Domain.TrivyOld.ConfigAuditReport;
using TrivyOperator.Dashboard.Domain.TrivyOld.ExposedSecretReport;
using TrivyOperator.Dashboard.Domain.TrivyOld.InfraAssessmentReport;
using TrivyOperator.Dashboard.Domain.TrivyOld.RbacAssessmentReport;
using TrivyOperator.Dashboard.Domain.TrivyOld.SbomReport;
using TrivyOperator.Dashboard.Domain.TrivyOld.Services.FileRepositoryOld;
using TrivyOperator.Dashboard.Domain.TrivyOld.Services.FileRepositoryOld.Abstractions;
using TrivyOperator.Dashboard.Domain.TrivyOld.VulnerabilityReport;
using TrivyOperator.Dashboard.Infrastructure.BackgroundQueues;
using TrivyOperator.Dashboard.Infrastructure.Caching.Distributed;
using TrivyOperator.Dashboard.Infrastructure.Caching.Distributed.Client;
using TrivyOperator.Dashboard.Infrastructure.Caching.Distributed.Client.Abstractions;
using TrivyOperator.Dashboard.Infrastructure.Caching.InMemoryOld;
using TrivyOperator.Dashboard.Infrastructure.Caching.InMemoryOld.Abstractions;
using TrivyOperator.Dashboard.Infrastructure.Clients.GitHub;
using TrivyOperator.Dashboard.Infrastructure.Clients.GitHub.Abstractions;
using TrivyOperator.Dashboard.Infrastructure.Clients.GitHub.Models;
using TrivyOperator.Dashboard.Infrastructure.Clients.Metrics;
using TrivyOperator.Dashboard.Infrastructure.Clients.Metrics.Abstractions;
using TrivyOperator.Dashboard.Infrastructure.FileRepository.Options;
using TrivyOperator.Dashboard.Infrastructure.K8s.ClientFactory;
using TrivyOperator.Dashboard.Infrastructure.K8s.ClientFactory.Abstractions;
using TrivyOperator.Dashboard.Infrastructure.K8s.Contexts;
using TrivyOperator.Dashboard.Infrastructure.K8s.Contexts.Abstractions;
using TrivyOperator.Dashboard.Infrastructure.K8s.CustomResources;
using TrivyOperator.Dashboard.Infrastructure.K8s.Services;
using TrivyOperator.Dashboard.Infrastructure.K8s.Services.Abstractions;
using TrivyOperator.Dashboard.Infrastructure.StaticResources.Services;
using TrivyOperator.Dashboard.Infrastructure.Trivy.Factories;
using TrivyOperator.Dashboard.Infrastructure.Trivy.Schema.VulnerabilityReports.Models;

namespace TrivyOperator.Dashboard.Application.Common;

public static class BuilderServicesExtensions
{
    public static ILogger? Logger { get; set; }

    public static void AddV1NamespaceServices(this IServiceCollection services, IConfiguration configuration)
    {
        bool useDefaultContext = configuration.GetValue<bool?>("Kubernetes:UseDefaultContext") ?? false;
        bool useNamespaceList = !string.IsNullOrWhiteSpace(configuration.GetValue<string?>("Kubernetes:NamespaceList"));
        bool useFileRepository = !string.IsNullOrWhiteSpace(configuration.GetValue<string?>("FileRepository:BasePath"));

        if (useFileRepository)
        {
            return;
        }

        if (!useDefaultContext)
        {
            if (!useNamespaceList)
            {
                Logger?.LogInformation("Using PassthroughCache for {kubernetesObjectType}", nameof(V1Namespace));
                services.AddSingleton<NamespaceService>();
                // TODO: change this to normal singleton registration and remove static ns watcher
                // services.AddSingleton<IClusterScopedResourceQueryService<V1Namespace, V1NamespaceList>>(sp =>
                //     sp.GetRequiredService<NamespaceService>()
                // );
                services.AddSingleton<IClusterScopedResourceService<V1Namespace, V1NamespaceList>>(sp =>
                    sp.GetRequiredService<NamespaceService>()
                );
            }
            else
            {
                Logger?.LogInformation(
                    "Using StaticNamespaceDomainService for {kubernetesObjectType}",
                    nameof(V1Namespace)
                );
                services.AddSingleton<IClusterScopedResourceService<V1Namespace, V1NamespaceList>,
                    StaticNamespaceService>();
                services.AddSingleton<IClusterScopedWatcher<V1Namespace>, StaticNamespaceWatcher>();
            }

            services
                .AddSingleton<IConcurrentDictionaryCache<V1Namespace>,
                    ClusterResourcePassthroughCache<V1Namespace, V1NamespaceList>>();

            services.AddScoped<IKubernetesNamespaceService, KubernetesNamespaceService>();

            return;
        }
        
        // else - useDefaultContext
        services.AddSingleton<IConcurrentDictionaryCache<V1Namespace>, ConcurrentDictionaryCache<V1Namespace>>();
        services.AddSingleton<IKubernetesBackgroundQueue<V1Namespace>, KubernetesBackgroundQueue<V1Namespace>>();
        if (!useNamespaceList)
        {
            Logger?.LogInformation("Using WatcherCache for {kubernetesObjectType}", nameof(V1Namespace));
            services.AddSingleton<NamespaceService>();
            services.AddSingleton<IClusterScopedResourceService<V1Namespace, V1NamespaceList>>(sp =>
                sp.GetRequiredService<NamespaceService>()
            );
            services.AddSingleton<IClusterScopedResourceService<V1Namespace, V1NamespaceList>>(sp =>
                sp.GetRequiredService<NamespaceService>()
            );
            services.AddSingleton<IClusterScopedWatcher<V1Namespace>,
                ClusterScopedWatcher<V1NamespaceList, V1Namespace, IKubernetesBackgroundQueue<V1Namespace>>>();
        }
        else
        {
            Logger?.LogInformation(
                "Using StaticNamespaceDomainService for {kubernetesObjectType}",
                nameof(V1Namespace)
            );
            services
                .AddSingleton<IClusterScopedResourceService<V1Namespace, V1NamespaceList>,
                    StaticNamespaceService>();
            services.AddSingleton<IClusterScopedWatcher<V1Namespace>, StaticNamespaceWatcher>();
        }

        services.AddSingleton<IClusterScopedKubernetesEventCoordinator,
            ClusterScopedKubernetesEventPipelineStarter<IKubernetesEventDispatcher<V1Namespace>,
                IClusterScopedWatcher<V1Namespace>, V1Namespace>>();
        services.AddSingleton<IKubernetesEventDispatcher<V1Namespace>,
            KubernetesEventDispatcher<V1Namespace, IKubernetesBackgroundQueue<V1Namespace>>>();
        services.AddSingleton<IKubernetesEventProcessor<V1Namespace>, NamespaceCacheRefresher>();
        services.AddSingleton<IKubernetesEventProcessor<V1Namespace>, WatcherStateEventProcessor<V1Namespace>>();
        services.AddSingleton<IKubernetesEventProcessor<V1Namespace>, WatcherStateAlertRefresh<V1Namespace>>();
        services.AddScoped<IKubernetesNamespaceService, KubernetesNamespaceService>();
    }

    public static void AddTrivyServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton<ICrdFactory, TrivyReportCrdFactory>();

        services.AddClusterScopedTrivyServices<OldClusterComplianceReportCr, IClusterComplianceReportService,
            ClusterComplianceReportNullService, ClusterComplianceReportService>(configuration);
        services.AddClusterScopedTrivyServices<OldClusterInfraAssessmentReportCr, IClusterInfraAssessmentReportService,
            ClusterInfraAssessmentReportNullService, ClusterInfraAssessmentReportService>(configuration);
        services.AddClusterScopedTrivyServices<OldClusterRbacAssessmentReportCr, IClusterRbacAssessmentReportService,
            ClusterRbacAssessmentReportNullService, ClusterRbacAssessmentReportService>(configuration);
        services.AddClusterScopedTrivyServices<OldClusterSbomReportCr, IClusterSbomReportService,
            ClusterSbomReportNullService, ClusterSbomReportService>(configuration);
        services.AddClusterScopedTrivyServices<OldClusterVulnerabilityReportCr, IClusterVulnerabilityReportService,
            ClusterVulnerabilityReportNullService, ClusterVulnerabilityReportService>(configuration);

        services.AddNamespacedTrivyServices<OldConfigAuditReportCr, IConfigAuditReportService,
            ConfigAuditReportNullService, ConfigAuditReportService>(configuration);
        services.AddNamespacedTrivyServices<OldExposedSecretReportCr, IExposedSecretReportService,
            ExposedSecretReportNullService, ExposedSecretReportService>(configuration);
        services.AddNamespacedTrivyServices<OldInfraAssessmentReportCr, IInfraAssessmentReportService,
            InfraAssessmentReportNullService, InfraAssessmentReportService>(configuration);
        services.AddNamespacedTrivyServices<OldRbacAssessmentReportCr, IRbacAssessmentReportService,
            RbacAssessmentReportNullService, RbacAssessmentReportService>(configuration);
        services.AddNamespacedTrivyServices<OldSbomReportCr, ISbomReportService, SbomReportNullService, SbomReportService>(
            configuration
        );
        services.AddNamespacedTrivyServices<OldVulnerabilityReportCr, IVulnerabilityReportService,
            VulnerabilityReportNullService, VulnerabilityReportService>(configuration);
    }

    private static void AddNamespacedTrivyServices<TNamespacedTrivyReportCr, TAppServiceInterface, TNullAppService,
        TAppService>(this IServiceCollection services, IConfiguration configuration)
        where TNamespacedTrivyReportCr : CustomResource, IKubernetesObject<V1ObjectMeta>, IMetadata<V1ObjectMeta>, new()
        where TAppServiceInterface : class
        where TNullAppService : class, TAppServiceInterface
        where TAppService : class, TAppServiceInterface
    {
        GetConfigFor<TNamespacedTrivyReportCr>(
            configuration,
            out string className,
            out bool useService,
            out bool useDefaultContext,
            out string basePath,
            out string subpath
        );

        if (!string.IsNullOrWhiteSpace(basePath) && useService && !string.IsNullOrWhiteSpace(subpath))
        {
            Logger?.LogInformation("Using FileRepository for {kubernetesObjectType}", className);
            services.AddSingleton<IFolderNameFactory, FolderNameFactory>();
            services.AddSingleton<
                IFileTrivyReportDomainService<TNamespacedTrivyReportCr>,
                FileTrivyReportDomainService<TNamespacedTrivyReportCr>>();

            services.AddSingleton<
                IConcurrentDictionaryCache<TNamespacedTrivyReportCr>,
                FileResourcePassthroughCache<TNamespacedTrivyReportCr>>();

            services.AddScoped<TAppServiceInterface, TAppService>();
            services
                .AddSingleton<
                    INamespacedResourceService<TNamespacedTrivyReportCr,
                        CustomResourceList<TNamespacedTrivyReportCr>>, FileTrivyReportPassThroughService<
                        TNamespacedTrivyReportCr, CustomResourceList<TNamespacedTrivyReportCr>>>();

            return;
        }

        if (!useService || (!string.IsNullOrWhiteSpace(basePath) && string.IsNullOrWhiteSpace(subpath)))
        {
            Logger?.LogInformation("Using NullService for {kubernetesObjectType}", className);
            services.AddScoped<TAppServiceInterface, TNullAppService>();
            services
                .AddTransient<IConcurrentDictionaryCache<TNamespacedTrivyReportCr>,
                    ConcurrentDictionaryCache<TNamespacedTrivyReportCr>>();
            return;
        }

        if (!useDefaultContext)
        {
            Logger?.LogInformation("Using PassthroughCache for {kubernetesObjectType}", className);
            services.AddSingleton<
                IConcurrentDictionaryCache<TNamespacedTrivyReportCr>, NamespacedResourcePassthroughCache<
                    TNamespacedTrivyReportCr, CustomResourceList<TNamespacedTrivyReportCr>>>();
        }
        else
        {
            Logger?.LogInformation("Using WatcherCache for {kubernetesObjectType}", className);
            services.AddSingleton<
                IConcurrentDictionaryCache<TNamespacedTrivyReportCr>,
                ConcurrentDictionaryCache<TNamespacedTrivyReportCr>>();
            services
                .AddSingleton<IKubernetesBackgroundQueue<TNamespacedTrivyReportCr>,
                    KubernetesBackgroundQueue<TNamespacedTrivyReportCr>>();
            if (typeof(TNamespacedTrivyReportCr).Name == "SbomReportCr")
            {
                services.AddSingleton<INamespacedWatcher<OldSbomReportCr>, SbomReportWatcher>();
            }
            else
            {
                services.AddSingleton<INamespacedWatcher<TNamespacedTrivyReportCr>, NamespacedWatcher<
                    CustomResourceList<TNamespacedTrivyReportCr>, TNamespacedTrivyReportCr,
                    IKubernetesBackgroundQueue<TNamespacedTrivyReportCr>>>();
            }

            services.AddSingleton<INamespacedKubernetesEventCoordinator,
                NamespacedKubernetesEventPipelineStarter<IKubernetesEventDispatcher<TNamespacedTrivyReportCr>,
                    INamespacedWatcher<TNamespacedTrivyReportCr>, TNamespacedTrivyReportCr>>();
            services.AddSingleton<IKubernetesEventDispatcher<TNamespacedTrivyReportCr>,
                KubernetesEventDispatcher<TNamespacedTrivyReportCr,
                    IKubernetesBackgroundQueue<TNamespacedTrivyReportCr>>>();
            services
                .AddSingleton<IKubernetesEventProcessor<TNamespacedTrivyReportCr>,
                    CacheRefresher<TNamespacedTrivyReportCr>>();
            services
                .AddSingleton<IKubernetesEventProcessor<TNamespacedTrivyReportCr>,
                    WatcherStateEventProcessor<TNamespacedTrivyReportCr>>();
            services
                .AddSingleton<IKubernetesEventProcessor<TNamespacedTrivyReportCr>,
                    WatcherStateAlertRefresh<TNamespacedTrivyReportCr>>();
        }

        services.AddScoped<TAppServiceInterface, TAppService>();

        services
            .AddSingleton<
                INamespacedResourceService<TNamespacedTrivyReportCr,
                    CustomResourceList<TNamespacedTrivyReportCr>>,
                NamespacedCustomResourceService<TNamespacedTrivyReportCr>>();
    }

    private static void AddClusterScopedTrivyServices<TClusterScopedTrivyReportCr, TAppServiceInterface, TNullAppService,
        TAppService>(this IServiceCollection services, IConfiguration configuration)
        where TClusterScopedTrivyReportCr : CustomResource, IKubernetesObject<V1ObjectMeta>, IMetadata<V1ObjectMeta>,
        new()
        where TAppServiceInterface : class
        where TNullAppService : class, TAppServiceInterface
        where TAppService : class, TAppServiceInterface
    {
        GetConfigFor<TClusterScopedTrivyReportCr>(
            configuration,
            out string className,
            out bool useService,
            out bool useDefaultContext,
            out string basePath,
            out string subpath
        );

        if (!string.IsNullOrWhiteSpace(basePath) && useService && !string.IsNullOrWhiteSpace(subpath))
        {
            Logger?.LogInformation("Using FileRepository for {kubernetesObjectType}", className);
            services.AddSingleton<IFolderNameFactory, FolderNameFactory>();
            services.AddSingleton<
                IFileTrivyReportDomainService<TClusterScopedTrivyReportCr>,
                FileTrivyReportDomainService<TClusterScopedTrivyReportCr>>();

            services.AddSingleton<
                IConcurrentDictionaryCache<TClusterScopedTrivyReportCr>,
                FileResourcePassthroughCache<TClusterScopedTrivyReportCr>>();

            services.AddScoped<TAppServiceInterface, TAppService>();

            return;
        }

        if (!useService || (!string.IsNullOrWhiteSpace(basePath) && string.IsNullOrWhiteSpace(subpath)))
        {
            Logger?.LogInformation("Using NullService for {kubernetesObjectType}", className);
            services.AddScoped<TAppServiceInterface, TNullAppService>();
            services
                .AddTransient<IConcurrentDictionaryCache<TClusterScopedTrivyReportCr>,
                    ConcurrentDictionaryCache<TClusterScopedTrivyReportCr>>();
            return;
        }

        if (!useDefaultContext)
        {
            Logger?.LogInformation("Using PassthroughCache for {kubernetesObjectType}", className);
            services.AddSingleton<
                IConcurrentDictionaryCache<TClusterScopedTrivyReportCr>, ClusterResourcePassthroughCache<
                    TClusterScopedTrivyReportCr, CustomResourceList<TClusterScopedTrivyReportCr>>>();
        }
        else
        {
            Logger?.LogInformation("Using WatcherCache for {kubernetesObjectType}", className);
            services
                .AddSingleton<IConcurrentDictionaryCache<TClusterScopedTrivyReportCr>,
                    ConcurrentDictionaryCache<TClusterScopedTrivyReportCr>>();
            services
                .AddSingleton<IKubernetesBackgroundQueue<TClusterScopedTrivyReportCr>,
                    KubernetesBackgroundQueue<TClusterScopedTrivyReportCr>>();
            services.AddSingleton<IClusterScopedWatcher<TClusterScopedTrivyReportCr>, ClusterScopedWatcher<
                CustomResourceList<TClusterScopedTrivyReportCr>, TClusterScopedTrivyReportCr,
                IKubernetesBackgroundQueue<TClusterScopedTrivyReportCr>>>();

            services.AddSingleton<IClusterScopedKubernetesEventCoordinator,
                ClusterScopedKubernetesEventPipelineStarter<IKubernetesEventDispatcher<TClusterScopedTrivyReportCr>,
                    IClusterScopedWatcher<TClusterScopedTrivyReportCr>, TClusterScopedTrivyReportCr>>();
            services.AddSingleton<IKubernetesEventDispatcher<TClusterScopedTrivyReportCr>,
                KubernetesEventDispatcher<TClusterScopedTrivyReportCr,
                    IKubernetesBackgroundQueue<TClusterScopedTrivyReportCr>>>();
            services
                .AddSingleton<IKubernetesEventProcessor<TClusterScopedTrivyReportCr>,
                    CacheRefresher<TClusterScopedTrivyReportCr>>();
            services
                .AddSingleton<IKubernetesEventProcessor<TClusterScopedTrivyReportCr>,
                    WatcherStateEventProcessor<TClusterScopedTrivyReportCr>>();
            services
                .AddSingleton<IKubernetesEventProcessor<TClusterScopedTrivyReportCr>,
                    WatcherStateAlertRefresh<TClusterScopedTrivyReportCr>>();
        }

        services.AddScoped<TAppServiceInterface, TAppService>();
        services
            .AddSingleton<
                IClusterScopedResourceService<TClusterScopedTrivyReportCr,
                    CustomResourceList<TClusterScopedTrivyReportCr>>,
                ClusterScopedCustomResourceService<TClusterScopedTrivyReportCr>>();
    }


    public static void AddWatcherStateServices(this IServiceCollection services)
    {
        services.AddSingleton<IConcurrentCache<WatcherKey, WatcherStateInfo>, ConcurrentCache<WatcherKey, WatcherStateInfo>>();
        //services.AddSingleton<IBackgroundQueue<WatcherStateInfo>, BackgroundQueue<WatcherStateInfo>>();
        services.AddScoped<IWatcherStatusService, WatcherStatusService>();
    }

    public static void AddHistoryServices(this IServiceCollection services, IConfiguration configuration)
    {
        bool useDefaultContext = configuration.GetValue<bool?>("Kubernetes:UseDefaultContext") ?? false;
        bool useFileRepository = !string.IsNullOrWhiteSpace(configuration.GetValue<string?>("FileRepository:BasePath"));
        bool isHistoryEnabled = configuration.GetValue<bool?>("History:Enabled") ?? false;
        
        services.Configure<VulnerabilityReportsHistoryOptions>(configuration.GetSection("History"));
        services.Configure<RetentionOptions>(configuration.GetSection("History").GetSection("Retention"));

        if (!isHistoryEnabled || !useDefaultContext || useFileRepository)
        {
            services.AddTransient<IVulnerabilityReportsHistoryService, VulnerabilityReportsHistoryNullService>();
            services.AddScoped<IVulnerabilityReportsHistoryStore, DistributedCacheVulnerabilityReportsHistoryNullStore>();
            return;
        }

        Logger?.LogInformation("Using DistributedCache for Vulnerability Reports History");
        
        services.Configure<DistributedCacheClientOptions>(configuration.GetSection("History").GetSection("DistributedCache"));
        services.Configure<DistributedCacheClientOptions>(configuration.GetSection("History").GetSection("DistributedCache").GetSection("RetryOptions"));

        services.AddSingleton<DistributedCacheConnectionProvider>();
        services.AddHostedService<DistributedCacheConnectionProvider>();
        
        services.AddSingleton<IDistributedCacheClientFactory, DistributedCacheClientFactory>();
        services.AddSingleton<IDistributedCacheExecutor, DistributedCacheExecutor>();
        
        services.AddScoped<IVulnerabilityReportsHistoryStore, DistributedCacheVulnerabilityReportsHistoryStore>();
        services.AddScoped<IVulnerabilityReportsHistoryRetentionService, VulnerabilityReportsHistoryRetentionService>();

        services.AddSingleton<IKubernetesEventProcessor<VulnerabilityReportCr>, VulnerabilityReportsHistoryRefresher>();
        services.AddTransient<IVulnerabilityReportsHistoryService, VulnerabilityReportsHistoryService>();
        
        services.AddHostedService<VulnerabilityReportsHistoryRetentionTimedHostedService>();
    }

    public static void AddAlertsServices(this IServiceCollection services)
    {
        services.AddSignalR();
        services.AddSingleton<IConcurrentCache<AlertKey, Alert>, ConcurrentCache<AlertKey, Alert>>();
        services.AddSingleton<IAlertPublisher, AlertPublisher>();
        services.AddTransient<IAlertsService, AlertsService>();
    }

    public static void AddCommons(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<BackgroundQueueOptions>(configuration.GetSection("Queues"));
        services.Configure<KubernetesOptions>(configuration.GetSection("Kubernetes"));
        services.Configure<FileRepositoryOptions>(configuration.GetSection("FileRepository"));
        services.Configure<WatchersOptions>(configuration.GetSection("Watchers"));
        services.Configure<FileExportOptions>(configuration.GetSection("FileExport"));
        services.Configure<GitHubOptions>(configuration.GetSection("GitHub"));

        services.AddHostedService<KubernetesEventPipelineHost>();
        services.AddHostedService<WatcherStateCacheTimedHostedService>();

        services.AddSingleton<IKubernetesClientFactory, KubernetesClientFactory>();
        //services.AddSingleton<IKubernetesContextResolver, HttpHeaderKubernetesContextResolver>();
        
        services.AddSingleton<IKubernetesContextResolver, DefaultKubernetesContextResolver>();
        services.AddSingleton<IKubernetesContextAccessor, KubernetesContextAccessor>();
        
        services.AddScoped<IKubernetesContextService, KubernetesContextService>();

        if (configuration.GetSection("GitHub").GetValue<bool>("ServerCheckForUpdates"))
        {
            services.AddHttpClient<IGitHubClient, GitHubClient>(client =>
                {
                    client.DefaultRequestHeaders.UserAgent.ParseAdd(Constants.UserAgentName);
                }
            );
            services.AddHostedService<GitHubReleaseCacheTimedHostedService>();
        }

        services.AddSingleton<IConcurrentCache<long, GitHubRelease>, ConcurrentCache<long, GitHubRelease>>();
        services.AddScoped<IAppVersionsService, AppVersionsService>();

        services.AddHealthChecks()
            .AddCheck<WatchersLivenessHealthCheck>("watchers-liveness")
            .AddCheck<WatchersReadinessHealthCheck>("watchers-readiness");

#if DEBUG
        //services.AddHostedService<SingleBucketTimedHostedService>();
        //services.AddSingleton<IHostedService>(provider =>
        //    new MultiBucketTimedHostedService(
        //        provider.GetRequiredService<ILogger<MultiBucketTimedHostedService>>(),
        //        provider.GetRequiredService<IAlertsService>(),
        //        "MultiBucket", ["Hey!", "Yo!", "No way, Jose!"], "subLevel|mama", 3));
        //services.AddSingleton<IHostedService>(provider =>
        //    new MultiBucketTimedHostedService(
        //        provider.GetRequiredService<ILogger<MultiBucketTimedHostedService>>(),
        //        provider.GetRequiredService<IAlertsService>(),
        //        "MultiBucket", ["Hey!", "Yo!", "Yes way, Jose!"], "subLevel|dada", 3));

        services.AddScoped<IRawDomainQueryService, RawDomainQueryService>();
#endif
    }

    public static void AddUiCommons(this IServiceCollection services) =>
        services.AddScoped<IBackendSettingsService, BackendSettingsService>();

    public static void AddOthers(this IServiceCollection services) =>
        services.AddScoped<ITrivyReportDependenciesService, TrivyReportDependenciesService>();

    public static void AddOpenTelemetry(
        this IServiceCollection services,
        IConfiguration configuration,
        string applicationName
    )
    {
        if (!(configuration.GetValue<bool?>("Enabled") ?? false))
        {
            services.AddSingleton<IMetricsClient>(_ => new MetricsClient(applicationName));
            return;
        }

        string fileVersion =
            Assembly.GetExecutingAssembly().GetCustomAttribute<AssemblyFileVersionAttribute>()?.Version ?? "0.0";
        string? otelEndpoint = configuration.GetValue<string>("OtelEndpoint");
        bool? isConsoleEnabled = configuration.GetValue<bool?>("ConsoleEnabled");
        bool? isAspNetCoreEnabled = configuration.GetValue<bool?>("AspNetCoreInstrumentationEnabled");
        bool? isRuntimeEnabled = configuration.GetValue<bool?>("RuntimeInstrumentationEnabled");
        int? metricsPort = configuration.GetValue<int>("PrometheusExporterPort");
        double[] histogramBounds = configuration.GetValue<double[]>("HistogramBoundsInMs") ?? [200, 500, 1000, 5000,];

        services.AddSingleton<IMetricsClient>(_ => new MetricsClient(applicationName));
        services.AddOpenTelemetry()
            .WithTracing(tracingBuilder =>
                {
                    tracingBuilder.SetResourceBuilder(
                            ResourceBuilder.CreateDefault()
                                .AddService(applicationName)
                                .AddAttributes(
                                    new Dictionary<string, object>
                                    {
                                        {
                                            "service.version", fileVersion
                                        },
                                    }
                                )
                        )
                        .AddHttpClientInstrumentation();
                    if (isConsoleEnabled ?? false)
                    {
                        tracingBuilder.AddConsoleExporter();
                    }

                    if (!string.IsNullOrWhiteSpace(otelEndpoint))
                    {
                        tracingBuilder.AddOtlpExporter(options =>
                            {
                                options.Endpoint = new Uri(otelEndpoint);
                                options.Protocol =
                                    (configuration.GetValue<string?>("OtelProtocol")?.ToLowerInvariant() ?? "grpc") ==
                                    "grpc"
                                        ? OtlpExportProtocol.Grpc : OtlpExportProtocol.HttpProtobuf;
                            }
                        );
                    }

                    if (isAspNetCoreEnabled ?? false)
                    {
                        tracingBuilder.AddAspNetCoreInstrumentation(options =>
                            {
                                options.Filter = context =>
                                {
                                    string? path = context.Request.Path.Value;
                                    return !((path?.StartsWith("/healthz") ?? false) ||
                                             (path?.StartsWith("/metrics") ?? false));
                                };
                            }
                        );
                    }
                }
            )
            .WithMetrics(metricsBuilder =>
                {
                    metricsBuilder.SetResourceBuilder(ResourceBuilder.CreateDefault().AddService(applicationName))
                        .AddView(
                            "*",
                            new ExplicitBucketHistogramConfiguration
                            {
                                Boundaries = histogramBounds,
                                // defaults: [ 0, 5, 10, 25, 50, 75, 100, 250, 500, 750, 1000, 2500, 5000, 7500, 10000 ]
                            }
                        )
                        .AddMeter($"{applicationName}.metrics");
                    if (!string.IsNullOrWhiteSpace(otelEndpoint))
                    {
                        metricsBuilder.AddOtlpExporter(options =>
                            {
                                options.Endpoint = new Uri(otelEndpoint);
                                options.Protocol =
                                    (configuration.GetValue<string?>("OTelProtocol")?.ToLowerInvariant() ?? "grpc") ==
                                    "grpc"
                                        ? OtlpExportProtocol.Grpc : OtlpExportProtocol.HttpProtobuf;
                            }
                        );
                    }

                    if (isConsoleEnabled ?? false)
                    {
                        metricsBuilder.AddConsoleExporter();
                    }

                    if (isAspNetCoreEnabled ?? false)
                    {
                        metricsBuilder.AddAspNetCoreInstrumentation();
                    }

                    if (isRuntimeEnabled ?? false)
                    {
                        metricsBuilder.AddRuntimeInstrumentation();
                    }

                    if (metricsPort is not null)
                    {
                        metricsBuilder.AddPrometheusExporter();
                    }
                }
            );
    }

    private static void GetConfigFor<T>(
        IConfiguration config,
        out string className,
        out bool useService,
        out bool useDefaultContext,
        out string basePath,
        out string subpath
    )
    {
        className = typeof(T).Name;

        string shortClassName = className.EndsWith("Cr", StringComparison.Ordinal) ? className[..^2] : className;

        useService = config.GetValue<bool?>($"Kubernetes:TrivyUse{shortClassName.Replace("Old", "")}") ?? false;

        useDefaultContext = config.GetValue<bool?>("Kubernetes:UseDefaultContext") ?? false;

        basePath = config.GetValue<string>("FileRepository:BasePath") ?? "";

        subpath = config.GetValue<string>($"FileRepository:{className}Subpath") ?? "";
    }
    
    // // wiring for distributed cache
    // services.Configure<DistributedCacheRetryOptions>(
    // configuration.GetSection("RetryOptions"));
    //
    // // 2. Factory (singleton)
    // services.AddSingleton<IDistributedCacheClientFactory>(sp =>
    // {
    //     var connString = configuration.GetConnectionString("DistributedCache")
    //                      ?? throw new InvalidOperationException("Missing cache connection string.");
    //     return new DistributedCacheClientFactory(connString);
    // });
    //
    // // 3. Executor (transient)
    // services.AddTransient<IDistributedCacheExecutor, DistributedCacheExecutor>();
}
