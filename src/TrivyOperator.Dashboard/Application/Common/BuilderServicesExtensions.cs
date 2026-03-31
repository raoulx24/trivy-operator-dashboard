using k8s;
using k8s.Models;
using OpenTelemetry.Exporter;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using System.Reflection;
using TrivyOperator.Dashboard.Application.Alerts.Services;
using TrivyOperator.Dashboard.Application.Alerts.Services.Abstractions;
using TrivyOperator.Dashboard.Application.AppVersions.Services;
using TrivyOperator.Dashboard.Application.AppVersions.Services.Abstractions;
using TrivyOperator.Dashboard.Application.AppVersions.Services.Options;
using TrivyOperator.Dashboard.Application.BackendSettings.Services;
using TrivyOperator.Dashboard.Application.BackendSettings.Services.Abstractions;
using TrivyOperator.Dashboard.Application.Common.BackgroundQueues;
using TrivyOperator.Dashboard.Application.Common.HealthChecks;
using TrivyOperator.Dashboard.Application.K8s.Services;
using TrivyOperator.Dashboard.Application.K8s.Services.BackgroundQueues;
using TrivyOperator.Dashboard.Application.K8s.Services.BackgroundQueues.Abstractions;
using TrivyOperator.Dashboard.Application.K8s.Services.CacheRefreshers;
using TrivyOperator.Dashboard.Application.K8s.Services.Contexts;
using TrivyOperator.Dashboard.Application.K8s.Services.Contexts.Abstractions;
using TrivyOperator.Dashboard.Application.K8s.Services.EventCoordinators;
using TrivyOperator.Dashboard.Application.K8s.Services.EventCoordinators.Abstractions;
using TrivyOperator.Dashboard.Application.K8s.Services.EventDispatchers;
using TrivyOperator.Dashboard.Application.K8s.Services.EventDispatchers.Abstractions;
using TrivyOperator.Dashboard.Application.K8s.Services.Namespaces;
using TrivyOperator.Dashboard.Application.K8s.Services.Namespaces.Abstractions;
using TrivyOperator.Dashboard.Application.K8s.Services.Options;
using TrivyOperator.Dashboard.Application.K8s.Services.RawDomain;
using TrivyOperator.Dashboard.Application.K8s.Services.RawDomain.Abstracts;
using TrivyOperator.Dashboard.Application.K8s.Services.WatcherEvents;
using TrivyOperator.Dashboard.Application.K8s.Services.Watchers;
using TrivyOperator.Dashboard.Application.K8s.Services.Watchers.Abstractions;
using TrivyOperator.Dashboard.Application.K8s.Services.WatcherStateAlertRefreshers;
using TrivyOperator.Dashboard.Application.K8s.Services.WatcherStates;
using TrivyOperator.Dashboard.Application.K8s.Services.WatcherStates.Abstractions;
using TrivyOperator.Dashboard.Application.Trivy.Services.ClusterComplianceReport;
using TrivyOperator.Dashboard.Application.Trivy.Services.ClusterComplianceReport.Abstractions;
using TrivyOperator.Dashboard.Application.Trivy.Services.ClusterInfraAssessmentReport;
using TrivyOperator.Dashboard.Application.Trivy.Services.ClusterInfraAssessmentReport.Abstractions;
using TrivyOperator.Dashboard.Application.Trivy.Services.ClusterRbacAssessmentReport;
using TrivyOperator.Dashboard.Application.Trivy.Services.ClusterRbacAssessmentReport.Abstractions;
using TrivyOperator.Dashboard.Application.Trivy.Services.ClusterSbomReport;
using TrivyOperator.Dashboard.Application.Trivy.Services.ClusterSbomReport.Abstractions;
using TrivyOperator.Dashboard.Application.Trivy.Services.ClusterVulnerabilityReport;
using TrivyOperator.Dashboard.Application.Trivy.Services.ClusterVulnerabilityReport.Abstractions;
using TrivyOperator.Dashboard.Application.Trivy.Services.ConfigAuditReport;
using TrivyOperator.Dashboard.Application.Trivy.Services.ConfigAuditReport.Abstractions;
using TrivyOperator.Dashboard.Application.Trivy.Services.ExposedSecretReport;
using TrivyOperator.Dashboard.Application.Trivy.Services.ExposedSecretReport.Abstractions;
using TrivyOperator.Dashboard.Application.Trivy.Services.InfraAssessmentReport;
using TrivyOperator.Dashboard.Application.Trivy.Services.InfraAssessmentReport.Abstractions;
using TrivyOperator.Dashboard.Application.Trivy.Services.Options;
using TrivyOperator.Dashboard.Application.Trivy.Services.RbacAssessmentReport;
using TrivyOperator.Dashboard.Application.Trivy.Services.RbacAssessmentReport.Abstractions;
using TrivyOperator.Dashboard.Application.Trivy.Services.SbomReport;
using TrivyOperator.Dashboard.Application.Trivy.Services.SbomReport.Abstractions;
using TrivyOperator.Dashboard.Application.Trivy.Services.VulnerabilityReport;
using TrivyOperator.Dashboard.Application.Trivy.Services.VulnerabilityReport.Abstractions;
using TrivyOperator.Dashboard.Application.TrivyReportDependencies.Services;
using TrivyOperator.Dashboard.Application.TrivyReportDependencies.Services.Abstractions;
using TrivyOperator.Dashboard.Domain.K8s;
using TrivyOperator.Dashboard.Domain.K8s.Abstractions;
using TrivyOperator.Dashboard.Domain.K8s.UpstreamAbstractions;
using TrivyOperator.Dashboard.Domain.Trivy.ClusterComplianceReport;
using TrivyOperator.Dashboard.Domain.Trivy.ClusterInfraAssessmentReport;
using TrivyOperator.Dashboard.Domain.Trivy.ClusterRbacAssessmentReport;
using TrivyOperator.Dashboard.Domain.Trivy.ClusterSbomReport;
using TrivyOperator.Dashboard.Domain.Trivy.ClusterVulnerabilityReport;
using TrivyOperator.Dashboard.Domain.Trivy.ConfigAuditReport;
using TrivyOperator.Dashboard.Domain.Trivy.CustomResources.Abstractions;
using TrivyOperator.Dashboard.Domain.Trivy.ExposedSecretReport;
using TrivyOperator.Dashboard.Domain.Trivy.InfraAssessmentReport;
using TrivyOperator.Dashboard.Domain.Trivy.RbacAssessmentReport;
using TrivyOperator.Dashboard.Domain.Trivy.SbomReport;
using TrivyOperator.Dashboard.Domain.Trivy.Services.FileRepository;
using TrivyOperator.Dashboard.Domain.Trivy.Services.FileRepository.Abstractions;
using TrivyOperator.Dashboard.Domain.Trivy.Services.FileRepository.Options;
using TrivyOperator.Dashboard.Domain.Trivy.Services.K8sApi;
using TrivyOperator.Dashboard.Domain.Trivy.Services.K8sApi.Abstractions;
using TrivyOperator.Dashboard.Domain.Trivy.VulnerabilityReport;
using TrivyOperator.Dashboard.Infrastructure.Caching;
using TrivyOperator.Dashboard.Infrastructure.Caching.Abstractions;
using TrivyOperator.Dashboard.Infrastructure.Clients;
using TrivyOperator.Dashboard.Infrastructure.Clients.Abstractions;
using TrivyOperator.Dashboard.Infrastructure.Clients.Models;
using TrivyOperator.Dashboard.Infrastructure.K8s;
using TrivyOperator.Dashboard.Infrastructure.K8s.Contexts;

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
                services.AddSingleton<NamespaceDomainService>();
                services.AddSingleton<IClusterScopedResourceQueryDomainService<V1Namespace, V1NamespaceList>>(sp =>
                    sp.GetRequiredService<NamespaceDomainService>()
                );
                services.AddSingleton<IClusterScopedResourceWatchDomainService<V1Namespace, V1NamespaceList>>(sp =>
                    sp.GetRequiredService<NamespaceDomainService>()
                );
            }
            else
            {
                Logger?.LogInformation(
                    "Using StaticNamespaceDomainService for {kubernetesObjectType}",
                    nameof(V1Namespace)
                );
                services.AddSingleton<IClusterScopedResourceQueryDomainService<V1Namespace, V1NamespaceList>,
                    StaticNamespaceDomainService>();
                services.AddSingleton<IClusterScopedWatcher<V1Namespace>, StaticNamespaceWatcher>();
            }

            services
                .AddSingleton<IConcurrentDictionaryCache<V1Namespace>,
                    ClusterResourcePassthroughCache<V1Namespace, V1NamespaceList>>();

            services.AddScoped<IKubernetesNamespaceService, KubernetesNamespaceService>();

            return;
        }

        services.AddSingleton<IConcurrentDictionaryCache<V1Namespace>, ConcurrentDictionaryCache<V1Namespace>>();
        services.AddSingleton<IKubernetesBackgroundQueue<V1Namespace>, KubernetesBackgroundQueue<V1Namespace>>();
        if (!useNamespaceList)
        {
            Logger?.LogInformation("Using WatcherCache for {kubernetesObjectType}", nameof(V1Namespace));
            services.AddSingleton<NamespaceDomainService>();
            services.AddSingleton<IClusterScopedResourceQueryDomainService<V1Namespace, V1NamespaceList>>(sp =>
                sp.GetRequiredService<NamespaceDomainService>()
            );
            services.AddSingleton<IClusterScopedResourceWatchDomainService<V1Namespace, V1NamespaceList>>(sp =>
                sp.GetRequiredService<NamespaceDomainService>()
            );
            services.AddSingleton<IClusterScopedWatcher<V1Namespace>,
                ClusterScopedWatcher<V1NamespaceList, V1Namespace, IKubernetesBackgroundQueue<V1Namespace>,
                    WatcherEvent<V1Namespace>>>();
        }
        else
        {
            Logger?.LogInformation(
                "Using StaticNamespaceDomainService for {kubernetesObjectType}",
                nameof(V1Namespace)
            );
            services
                .AddSingleton<IClusterScopedResourceQueryDomainService<V1Namespace, V1NamespaceList>,
                    StaticNamespaceDomainService>();
            services.AddSingleton<IClusterScopedWatcher<V1Namespace>, StaticNamespaceWatcher>();
        }

        services.AddSingleton<IClusterScopedKubernetesEventCoordinator,
            ClusterScopedKubernetesEventCoordinator<IKubernetesEventDispatcher<V1Namespace>,
                IClusterScopedWatcher<V1Namespace>, V1Namespace>>();
        services.AddSingleton<IKubernetesEventDispatcher<V1Namespace>,
            KubernetesEventDispatcher<V1Namespace, IKubernetesBackgroundQueue<V1Namespace>>>();
        services.AddSingleton<IKubernetesEventProcessor<V1Namespace>, NamespaceCacheRefresher>();
        services.AddSingleton<IKubernetesEventProcessor<V1Namespace>, WatcherState<V1Namespace>>();
        services.AddSingleton<IKubernetesEventProcessor<V1Namespace>, WatcherStateAlertRefresh<V1Namespace>>();
        services.AddScoped<IKubernetesNamespaceService, KubernetesNamespaceService>();
    }

    public static void AddTrivyServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton<ICustomResourceDefinitionFactory, CustomResourceDefinitionFactory>();

        services.AddClusterScopedTrivyServices<ClusterComplianceReportCr, IClusterComplianceReportService,
            ClusterComplianceReportNullService, ClusterComplianceReportService>(configuration);
        services.AddClusterScopedTrivyServices<ClusterInfraAssessmentReportCr, IClusterInfraAssessmentReportService,
            ClusterInfraAssessmentReportNullService, ClusterInfraAssessmentReportService>(configuration);
        services.AddClusterScopedTrivyServices<ClusterRbacAssessmentReportCr, IClusterRbacAssessmentReportService,
            ClusterRbacAssessmentReportNullService, ClusterRbacAssessmentReportService>(configuration);
        services.AddClusterScopedTrivyServices<ClusterSbomReportCr, IClusterSbomReportService,
            ClusterSbomReportNullService, ClusterSbomReportService>(configuration);
        services.AddClusterScopedTrivyServices<ClusterVulnerabilityReportCr, IClusterVulnerabilityReportService,
            ClusterVulnerabilityReportNullService, ClusterVulnerabilityReportService>(configuration);

        services.AddNamespacedTrivyServices<ConfigAuditReportCr, IConfigAuditReportService,
            ConfigAuditReportNullService, ConfigAuditReportService>(configuration);
        services.AddNamespacedTrivyServices<ExposedSecretReportCr, IExposedSecretReportService,
            ExposedSecretReportNullService, ExposedSecretReportService>(configuration);
        services.AddNamespacedTrivyServices<InfraAssessmentReportCr, IInfraAssessmentReportService,
            InfraAssessmentReportNullService, InfraAssessmentReportService>(configuration);
        services.AddNamespacedTrivyServices<RbacAssessmentReportCr, IRbacAssessmentReportService,
            RbacAssessmentReportNullService, RbacAssessmentReportService>(configuration);
        services.AddNamespacedTrivyServices<SbomReportCr, ISbomReportService, SbomReportNullService, SbomReportService>(
            configuration
        );
        services.AddNamespacedTrivyServices<VulnerabilityReportCr, IVulnerabilityReportService,
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
                    INamespacedResourceWatchDomainService<TNamespacedTrivyReportCr,
                        CustomResourceList<TNamespacedTrivyReportCr>>, FileTrivyReportPassThroughDomainService<
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
                services.AddSingleton<INamespacedWatcher<SbomReportCr>, SbomReportWatcher>();
            }
            else
            {
                services.AddSingleton<INamespacedWatcher<TNamespacedTrivyReportCr>, NamespacedWatcher<
                    CustomResourceList<TNamespacedTrivyReportCr>, TNamespacedTrivyReportCr,
                    IKubernetesBackgroundQueue<TNamespacedTrivyReportCr>, WatcherEvent<TNamespacedTrivyReportCr>>>();
            }

            services.AddSingleton<INamespacedKubernetesEventCoordinator,
                NamespacedKubernetesEventCoordinator<IKubernetesEventDispatcher<TNamespacedTrivyReportCr>,
                    INamespacedWatcher<TNamespacedTrivyReportCr>, TNamespacedTrivyReportCr>>();
            services.AddSingleton<IKubernetesEventDispatcher<TNamespacedTrivyReportCr>,
                KubernetesEventDispatcher<TNamespacedTrivyReportCr,
                    IKubernetesBackgroundQueue<TNamespacedTrivyReportCr>>>();
            services
                .AddSingleton<IKubernetesEventProcessor<TNamespacedTrivyReportCr>,
                    CacheRefresher<TNamespacedTrivyReportCr>>();
            services
                .AddSingleton<IKubernetesEventProcessor<TNamespacedTrivyReportCr>,
                    WatcherState<TNamespacedTrivyReportCr>>();
            services
                .AddSingleton<IKubernetesEventProcessor<TNamespacedTrivyReportCr>,
                    WatcherStateAlertRefresh<TNamespacedTrivyReportCr>>();
        }

        services.AddScoped<TAppServiceInterface, TAppService>();

        services
            .AddSingleton<
                INamespacedResourceWatchDomainService<TNamespacedTrivyReportCr,
                    CustomResourceList<TNamespacedTrivyReportCr>>,
                NamespacedTrivyReportDomainService<TNamespacedTrivyReportCr>>();
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
                IKubernetesBackgroundQueue<TClusterScopedTrivyReportCr>, WatcherEvent<TClusterScopedTrivyReportCr>>>();

            services.AddSingleton<IClusterScopedKubernetesEventCoordinator,
                ClusterScopedKubernetesEventCoordinator<IKubernetesEventDispatcher<TClusterScopedTrivyReportCr>,
                    IClusterScopedWatcher<TClusterScopedTrivyReportCr>, TClusterScopedTrivyReportCr>>();
            services.AddSingleton<IKubernetesEventDispatcher<TClusterScopedTrivyReportCr>,
                KubernetesEventDispatcher<TClusterScopedTrivyReportCr,
                    IKubernetesBackgroundQueue<TClusterScopedTrivyReportCr>>>();
            services
                .AddSingleton<IKubernetesEventProcessor<TClusterScopedTrivyReportCr>,
                    CacheRefresher<TClusterScopedTrivyReportCr>>();
            services
                .AddSingleton<IKubernetesEventProcessor<TClusterScopedTrivyReportCr>,
                    WatcherState<TClusterScopedTrivyReportCr>>();
            services
                .AddSingleton<IKubernetesEventProcessor<TClusterScopedTrivyReportCr>,
                    WatcherStateAlertRefresh<TClusterScopedTrivyReportCr>>();
        }

        services.AddScoped<TAppServiceInterface, TAppService>();
        services
            .AddSingleton<
                IClusterScopedResourceWatchDomainService<TClusterScopedTrivyReportCr,
                    CustomResourceList<TClusterScopedTrivyReportCr>>,
                ClusterScopedTrivyReportDomainService<TClusterScopedTrivyReportCr>>();
    }


    public static void AddWatcherStateServices(this IServiceCollection services)
    {
        services.AddSingleton<IConcurrentCache<string, WatcherStateInfo>, ConcurrentCache<string, WatcherStateInfo>>();
        //services.AddSingleton<IBackgroundQueue<WatcherStateInfo>, BackgroundQueue<WatcherStateInfo>>();
        services.AddScoped<IWatcherStatusService, WatcherStatusService>();
    }

    public static void AddAlertsServices(this IServiceCollection services)
    {
        services.AddSignalR();
        services.AddSingleton<IConcurrentCache<string, Alert>, ConcurrentCache<string, Alert>>();
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

        services.AddHostedService<KubernetesEventCoordinatorsHandlerHostedService>();
        services.AddHostedService<WatcherStateCacheTimedHostedService>();

        services.AddSingleton<IKubernetesClientFactory, KubernetesClientFactory>();
        services.AddScoped<IKubernetesContextProvider, HttpHeaderKubernetesContextProvider>();
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
                                    (configuration.GetValue<string?>("OtelProtocol")?.ToLowerInvariant() ?? "grpc") ==
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

        useService = config.GetValue<bool?>($"Kubernetes:TrivyUse{shortClassName}") ?? false;

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
