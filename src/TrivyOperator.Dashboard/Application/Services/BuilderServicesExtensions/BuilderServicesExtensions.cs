using k8s;
using k8s.Models;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using System.Reflection;
using TrivyOperator.Dashboard.Application.Caching;
using TrivyOperator.Dashboard.Application.HealthChecks;
using TrivyOperator.Dashboard.Application.Services.Alerts;
using TrivyOperator.Dashboard.Application.Services.Alerts.Abstractions;
using TrivyOperator.Dashboard.Application.Services.AppVersions;
using TrivyOperator.Dashboard.Application.Services.AppVersions.Abstractions;
using TrivyOperator.Dashboard.Application.Services.BackendSettings;
using TrivyOperator.Dashboard.Application.Services.BackendSettings.Abstractions;
using TrivyOperator.Dashboard.Application.Services.BackgroundQueues;
using TrivyOperator.Dashboard.Application.Services.BackgroundQueues.Abstractions;
using TrivyOperator.Dashboard.Application.Services.CacheRefreshers;
using TrivyOperator.Dashboard.Application.Services.KubernetesContexts;
using TrivyOperator.Dashboard.Application.Services.KubernetesContexts.Abstractions;
using TrivyOperator.Dashboard.Application.Services.KubernetesEventCoordinators;
using TrivyOperator.Dashboard.Application.Services.KubernetesEventCoordinators.Abstractions;
using TrivyOperator.Dashboard.Application.Services.KubernetesEventDispatchers;
using TrivyOperator.Dashboard.Application.Services.KubernetesEventDispatchers.Abstractions;
using TrivyOperator.Dashboard.Application.Services.Namespaces;
using TrivyOperator.Dashboard.Application.Services.Namespaces.Abstractions;
using TrivyOperator.Dashboard.Application.Services.Options;
using TrivyOperator.Dashboard.Application.Services.RawDomainQueryServices;
using TrivyOperator.Dashboard.Application.Services.RawDomainQueryServices.Abstracts;
using TrivyOperator.Dashboard.Application.Services.Trivy.ClusterComplianceReport;
using TrivyOperator.Dashboard.Application.Services.Trivy.ClusterComplianceReport.Abstractions;
using TrivyOperator.Dashboard.Application.Services.Trivy.ClusterRbacAssessmentReport;
using TrivyOperator.Dashboard.Application.Services.Trivy.ClusterRbacAssessmentReport.Abstractions;
using TrivyOperator.Dashboard.Application.Services.Trivy.ClusterSbomReport;
using TrivyOperator.Dashboard.Application.Services.Trivy.ClusterSbomReport.Abstractions;
using TrivyOperator.Dashboard.Application.Services.Trivy.ClusterVulnerabilityReport;
using TrivyOperator.Dashboard.Application.Services.Trivy.ClusterVulnerabilityReport.Abstractions;
using TrivyOperator.Dashboard.Application.Services.Trivy.ConfigAuditReport;
using TrivyOperator.Dashboard.Application.Services.Trivy.ConfigAuditReport.Abstractions;
using TrivyOperator.Dashboard.Application.Services.Trivy.ExposedSecretReport;
using TrivyOperator.Dashboard.Application.Services.Trivy.ExposedSecretReport.Abstractions;
using TrivyOperator.Dashboard.Application.Services.Trivy.InfraAssessmentReport;
using TrivyOperator.Dashboard.Application.Services.Trivy.InfraAssessmentReport.Abstractions;
using TrivyOperator.Dashboard.Application.Services.Trivy.RbacAssessmentReport;
using TrivyOperator.Dashboard.Application.Services.Trivy.RbacAssessmentReport.Abstractions;
using TrivyOperator.Dashboard.Application.Services.Trivy.SbomReport;
using TrivyOperator.Dashboard.Application.Services.Trivy.SbomReport.Abstractions;
using TrivyOperator.Dashboard.Application.Services.Trivy.VulnerabilityReport;
using TrivyOperator.Dashboard.Application.Services.Trivy.VulnerabilityReport.Abstractions;
using TrivyOperator.Dashboard.Application.Services.TrivyReportDependencies;
using TrivyOperator.Dashboard.Application.Services.TrivyReportDependencies.Abstractions;
using TrivyOperator.Dashboard.Application.Services.WatcherEvents;
using TrivyOperator.Dashboard.Application.Services.Watchers;
using TrivyOperator.Dashboard.Application.Services.Watchers.Abstractions;
using TrivyOperator.Dashboard.Application.Services.WatcherStateAlertRefreshers;
using TrivyOperator.Dashboard.Application.Services.WatcherStates;
using TrivyOperator.Dashboard.Application.Services.WatcherStates.Abstractions;
using TrivyOperator.Dashboard.Domain.Services;
using TrivyOperator.Dashboard.Domain.Services.Abstractions;
using TrivyOperator.Dashboard.Domain.Trivy.ClusterComplianceReport;
using TrivyOperator.Dashboard.Domain.Trivy.ClusterRbacAssessmentReport;
using TrivyOperator.Dashboard.Domain.Trivy.ClusterSbomReport;
using TrivyOperator.Dashboard.Domain.Trivy.ClusterVulnerabilityReport;
using TrivyOperator.Dashboard.Domain.Trivy.ConfigAuditReport;
using TrivyOperator.Dashboard.Domain.Trivy.CustomResources.Abstractions;
using TrivyOperator.Dashboard.Domain.Trivy.ExposedSecretReport;
using TrivyOperator.Dashboard.Domain.Trivy.InfraAssessmentReport;
using TrivyOperator.Dashboard.Domain.Trivy.RbacAssessmentReport;
using TrivyOperator.Dashboard.Domain.Trivy.SbomReport;
using TrivyOperator.Dashboard.Domain.Trivy.VulnerabilityReport;
using TrivyOperator.Dashboard.Infrastructure.Abstractions;
using TrivyOperator.Dashboard.Infrastructure.Clients;
using TrivyOperator.Dashboard.Infrastructure.Clients.Models;
using TrivyOperator.Dashboard.Infrastructure.Contexts;
using TrivyOperator.Dashboard.Infrastructure.Services;

namespace TrivyOperator.Dashboard.Application.Services.BuilderServicesExtensions;

public static class BuilderServicesExtensions
{
    public static void AddV1NamespaceServices(this IServiceCollection services, IConfiguration kubernetesConfiguration)
    {
        services.AddSingleton<IConcurrentDictionaryCache<V1Namespace>, ConcurrentDictionaryCache<V1Namespace>>();
        services.AddSingleton<IKubernetesBackgroundQueue<V1Namespace>, KubernetesBackgroundQueue<V1Namespace>>();
        if (string.IsNullOrWhiteSpace(kubernetesConfiguration.GetValue<string>("NamespaceList")))
        {
            services.AddSingleton<NamespaceDomainService>();
            services.AddSingleton<IClusterScopedResourceQueryDomainService<V1Namespace, V1NamespaceList>>(
                sp => sp.GetRequiredService<NamespaceDomainService>());
            services.AddSingleton<IClusterScopedResourceWatchDomainService<V1Namespace, V1NamespaceList>>(
                sp => sp.GetRequiredService<NamespaceDomainService>());
            services.AddSingleton<IClusterScopedWatcher<V1Namespace>,
                ClusterScopedWatcher<V1NamespaceList, V1Namespace, IKubernetesBackgroundQueue<V1Namespace>,
                    WatcherEvent<V1Namespace>>>();
        }
        else
        {
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
        services.AddScoped<INamespaceService, NamespaceService>();
        
    }

    public static void AddTrivyServices(this IServiceCollection services, IConfiguration kubernetesConfiguration)
    {
        services.AddSingleton<ICustomResourceDefinitionFactory, CustomResourceDefinitionFactory>();

        services.AddClusterScopedService<ClusterComplianceReportCr, IClusterComplianceReportService,
            ClusterComplianceReportNullService, ClusterComplianceReportService>(kubernetesConfiguration, "TrivyUseClusterComplianceReport");
        services.AddClusterScopedService<ClusterRbacAssessmentReportCr, IClusterRbacAssessmentReportService,
            ClusterRbacAssessmentReportNullService, ClusterRbacAssessmentReportService>(kubernetesConfiguration, "TrivyUseClusterRbacAssessmentReport");
        services.AddClusterScopedService<ClusterSbomReportCr, IClusterSbomReportService,
            ClusterSbomReportNullService, ClusterSbomReportService>(kubernetesConfiguration, "TrivyUseClusterSbomReport");
        services.AddClusterScopedService<ClusterVulnerabilityReportCr, IClusterVulnerabilityReportService, 
            ClusterVulnerabilityReportNullService, ClusterVulnerabilityReportService>(kubernetesConfiguration, "TrivyUseClusterVulnerabilityReport");

        services.AddNamespacedService<ConfigAuditReportCr, IConfigAuditReportService, 
            ConfigAuditReportNullService, ConfigAuditReportService>(kubernetesConfiguration, "TrivyUseConfigAuditReport");
        services.AddNamespacedService<ExposedSecretReportCr, IExposedSecretReportService, 
            ExposedSecretReportNullService, ExposedSecretReportService>(kubernetesConfiguration, "TrivyUseExposedSecretReport");
        services.AddNamespacedService<InfraAssessmentReportCr, IInfraAssessmentReportService,
            InfraAssessmentReportNullService, InfraAssessmentReportService>(kubernetesConfiguration, "TrivyUseInfraAssessmentReport");
        services.AddNamespacedService<RbacAssessmentReportCr, IRbacAssessmentReportService,
            RbacAssessmentReportNullService, RbacAssessmentReportService>(kubernetesConfiguration, "TrivyUseRbacAssessmentReport");
        services.AddNamespacedService<SbomReportCr, ISbomReportService, 
            SbomReportNullService, SbomReportService>(kubernetesConfiguration, "TrivyUseSbomReport");
        services.AddNamespacedService<VulnerabilityReportCr, IVulnerabilityReportService, 
            VulnerabilityReportNullService, VulnerabilityReportService>(kubernetesConfiguration, "TrivyUseVulnerabilityReport");
    }

    public static void AddNamespacedService<TNamespacedTrivyReportCr, TAppServiceInterface,
        TNullAppService, TAppService>(this IServiceCollection services, IConfiguration kubernetesConfiguration, string trivyUseReportParamName)
        where TNamespacedTrivyReportCr : CustomResource, IKubernetesObject<V1ObjectMeta>, IMetadata<V1ObjectMeta>, new()
        where TAppServiceInterface : class
        where TNullAppService : class, TAppServiceInterface
        where TAppService : class, TAppServiceInterface
    {
        bool? useServices = kubernetesConfiguration.GetValue<bool?>(trivyUseReportParamName);
        bool? useDefaultContext = kubernetesConfiguration.GetValue<bool?>("UseDefaultContext");

        if (useServices == null || !(bool)useServices)
        {
            services.AddScoped<TAppServiceInterface, TNullAppService>();
            services.AddTransient<IConcurrentDictionaryCache<TNamespacedTrivyReportCr>, ConcurrentDictionaryCache<TNamespacedTrivyReportCr>>();
            return;
        }

        if (useDefaultContext == null || !(bool)useDefaultContext)
        {
            services.AddSingleton<
                IConcurrentDictionaryCache<TNamespacedTrivyReportCr>,
                NamespacedResourceQueryCache<TNamespacedTrivyReportCr, CustomResourceList<TNamespacedTrivyReportCr>>>();
        }
        else
        {
            services.AddSingleton<
                IConcurrentDictionaryCache<TNamespacedTrivyReportCr>,
                ConcurrentDictionaryCache<TNamespacedTrivyReportCr>>();
            services.AddSingleton<IKubernetesBackgroundQueue<TNamespacedTrivyReportCr>, KubernetesBackgroundQueue<TNamespacedTrivyReportCr>>();
            services.AddSingleton<INamespacedWatcher<TNamespacedTrivyReportCr>,
                NamespacedWatcher<CustomResourceList<TNamespacedTrivyReportCr>, TNamespacedTrivyReportCr,
                    IKubernetesBackgroundQueue<TNamespacedTrivyReportCr>, WatcherEvent<TNamespacedTrivyReportCr>>>();
            services.AddSingleton<INamespacedKubernetesEventCoordinator,
            NamespacedKubernetesEventCoordinator<IKubernetesEventDispatcher<TNamespacedTrivyReportCr>,
                INamespacedWatcher<TNamespacedTrivyReportCr>, TNamespacedTrivyReportCr>>();
            services.AddSingleton<IKubernetesEventDispatcher<TNamespacedTrivyReportCr>,
                KubernetesEventDispatcher<TNamespacedTrivyReportCr, IKubernetesBackgroundQueue<TNamespacedTrivyReportCr>>>();
            services.AddSingleton<IKubernetesEventProcessor<TNamespacedTrivyReportCr>, CacheRefresher<TNamespacedTrivyReportCr>>();
            services.AddSingleton<IKubernetesEventProcessor<TNamespacedTrivyReportCr>, WatcherState<TNamespacedTrivyReportCr>>();
            services.AddSingleton<IKubernetesEventProcessor<TNamespacedTrivyReportCr>, WatcherStateAlertRefresh<TNamespacedTrivyReportCr>>();

        }

        services.AddScoped<TAppServiceInterface, TAppService>();

        services.AddSingleton<INamespacedResourceWatchDomainService<TNamespacedTrivyReportCr, CustomResourceList<TNamespacedTrivyReportCr>>,
            NamespacedTrivyReportDomainService<TNamespacedTrivyReportCr>>();
    }

    public static void AddClusterScopedService<TClusterScopedTrivyReportCr, TAppServiceInterface,
        TNullAppService, TAppService>(this IServiceCollection services, IConfiguration kubernetesConfiguration, string trivyUseReportParamName)
        where TClusterScopedTrivyReportCr : CustomResource, IKubernetesObject<V1ObjectMeta>, IMetadata<V1ObjectMeta>, new()
        where TAppServiceInterface : class
        where TNullAppService : class, TAppServiceInterface
        where TAppService : class, TAppServiceInterface
    {
        bool? useServices = kubernetesConfiguration.GetValue<bool?>(trivyUseReportParamName);
        bool? useDefaultContext = kubernetesConfiguration.GetValue<bool?>("UseDefaultContext");

        if (useServices == null || !(bool)useServices)
        {
            services.AddScoped<TAppServiceInterface, TNullAppService>();
            services.AddTransient<IConcurrentDictionaryCache<TClusterScopedTrivyReportCr>, ConcurrentDictionaryCache<TClusterScopedTrivyReportCr>>();
            return;
        }

        if (useDefaultContext == null || !(bool)useDefaultContext)
        {
            services.AddSingleton<
                IConcurrentDictionaryCache<TClusterScopedTrivyReportCr>,
                ClusterResourceQueryCache<TClusterScopedTrivyReportCr, CustomResourceList<TClusterScopedTrivyReportCr>>>();
        }
        else
        {
            services
                .AddSingleton<IConcurrentDictionaryCache<TClusterScopedTrivyReportCr>,
                    ConcurrentDictionaryCache<TClusterScopedTrivyReportCr>>();
            services
                .AddSingleton<IKubernetesBackgroundQueue<TClusterScopedTrivyReportCr>, KubernetesBackgroundQueue<TClusterScopedTrivyReportCr>>();
            services.AddSingleton<IClusterScopedWatcher<TClusterScopedTrivyReportCr>, ClusterScopedWatcher<
                CustomResourceList<TClusterScopedTrivyReportCr>, TClusterScopedTrivyReportCr,
                IKubernetesBackgroundQueue<TClusterScopedTrivyReportCr>, WatcherEvent<TClusterScopedTrivyReportCr>>>();

            services.AddSingleton<IClusterScopedKubernetesEventCoordinator,
                ClusterScopedKubernetesEventCoordinator<IKubernetesEventDispatcher<TClusterScopedTrivyReportCr>,
                    IClusterScopedWatcher<TClusterScopedTrivyReportCr>, TClusterScopedTrivyReportCr>>();
            services.AddSingleton<IKubernetesEventDispatcher<TClusterScopedTrivyReportCr>,
                KubernetesEventDispatcher<TClusterScopedTrivyReportCr, IKubernetesBackgroundQueue<TClusterScopedTrivyReportCr>>>();
            services.AddSingleton<IKubernetesEventProcessor<TClusterScopedTrivyReportCr>, CacheRefresher<TClusterScopedTrivyReportCr>>();
            services.AddSingleton<IKubernetesEventProcessor<TClusterScopedTrivyReportCr>, WatcherState<TClusterScopedTrivyReportCr>>();
            services.AddSingleton<IKubernetesEventProcessor<TClusterScopedTrivyReportCr>, WatcherStateAlertRefresh<TClusterScopedTrivyReportCr>>();
        }
        
        services.AddScoped<TAppServiceInterface, TAppService>();
        services.AddSingleton<IClusterScopedResourceWatchDomainService<TClusterScopedTrivyReportCr, CustomResourceList<TClusterScopedTrivyReportCr>>,
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

    public static void AddCommons(
        this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<BackgroundQueueOptions>(configuration.GetSection("Queues"));
        services.Configure<KubernetesOptions>(configuration.GetSection("Kubernetes"));
        services.Configure<WatchersOptions>(configuration.GetSection("Watchers"));
        services.Configure<FileExportOptions>(configuration.GetSection("FileExport"));
        services.Configure<GitHubOptions>(configuration.GetSection("GitHub"));

        services.AddHostedService<KubernetesEventCoordinatorsHandlerHostedService>();
        services.AddHostedService<WatcherStateCacheTimedHostedService>();

        services.AddSingleton<IKubernetesClientFactory, KubernetesClientFactory>();
        //services.AddScoped<IKubernetesContextProvider, DefaultKubernetesContextProvider>();
        services.AddScoped<IKubernetesContextProvider, HttpHeaderKubernetesContesxtProvider>();
        services.AddScoped<IKubernetesContextService, KubernetesContextService>();

        if (configuration.GetSection("GitHub").GetValue<bool>("ServerCheckForUpdates"))
        {
            services.AddHttpClient<IGitHubClient, GitHubClient>(client =>
            {
                client.DefaultRequestHeaders.UserAgent.ParseAdd(Constants.UserAgentName);
            });
            services.AddHostedService<GitHubReleaseCacheTimedHostedService>();
        }
        services.AddSingleton<IConcurrentCache<long, GitHubRelease>, ConcurrentCache<long, GitHubRelease>>();
        services.AddScoped<IAppVersionService, AppVersionService>();

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

    public static void AddOthers(this IServiceCollection services)
    {
        services.AddScoped<ITrivyReportDependency, TrivyReportDependency>();
    }

    public static void AddOpenTelemetry(this IServiceCollection services, IConfiguration configuration, string applicationName)
    {
        if ((configuration.GetValue<bool?>("Enabled") ?? false) == false)
        {
            services.AddSingleton<IMetricsService>(provider => new MetricsService(applicationName));
            return;
        }

        string fileVersion = Assembly.GetExecutingAssembly().GetCustomAttribute<AssemblyFileVersionAttribute>()?.Version ?? "0.0";
        string? otelEndpoint = configuration.GetValue<string>("OtelEndpoint");
        bool? isConsoleEnabled = configuration.GetValue<bool?>("ConsoleEnabled");
        bool? isAspNetCoreEnabled = configuration.GetValue<bool?>("AspNetCoreInstrumentationEnabled");
        bool? isRuntimeEnabled = configuration.GetValue<bool?>("RuntimeInstrumentationEnabled");
        int? metricsPort = configuration.GetValue<int>("PrometheusExporterPort");
        double[]? histogramBounds = configuration.GetValue<double[]>("HistogramBoundsInMs") ?? [200, 500, 1000, 5000];

        services.AddSingleton<IMetricsService>(provider => new MetricsService(applicationName));
        services.AddOpenTelemetry()
            .WithTracing(tracingBuilder =>
            {
                tracingBuilder
                    .SetResourceBuilder(ResourceBuilder.CreateDefault()
                        .AddService(applicationName)
                        .AddAttributes(new Dictionary<string, object>
                        {
                            { "service.version", fileVersion }
                        }))
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
                        options.Protocol = (configuration.GetValue<string?>("OtelProtocol")?.ToLowerInvariant() ?? "grpc") == "grpc"
                            ? OpenTelemetry.Exporter.OtlpExportProtocol.Grpc
                            : OpenTelemetry.Exporter.OtlpExportProtocol.HttpProtobuf;
                    });
                }
                if (isAspNetCoreEnabled ?? false)
                {
                    tracingBuilder.AddAspNetCoreInstrumentation(options =>
                    {
                        options.Filter = context =>
                        {
                            var path = context.Request.Path.Value;
                            return !((path?.StartsWith("/healthz") ?? false) || (path?.StartsWith("/metrics") ?? false));
                        };
                    });
                }

            })
            .WithMetrics(metricsBuilder =>
            {
                metricsBuilder
                    .SetResourceBuilder(ResourceBuilder.CreateDefault().AddService(applicationName))
                    .AddView("*", new ExplicitBucketHistogramConfiguration
                    {
                        Boundaries = histogramBounds,
                        // defaults: [ 0, 5, 10, 25, 50, 75, 100, 250, 500, 750, 1000, 2500, 5000, 7500, 10000 ]
                    })
                    .AddMeter($"{applicationName}.metrics");
                if (!string.IsNullOrWhiteSpace(otelEndpoint))
                {
                    metricsBuilder.AddOtlpExporter(options =>
                    {
                        options.Endpoint = new Uri(otelEndpoint);
                        options.Protocol = (configuration.GetValue<string?>("OtelProtocol")?.ToLowerInvariant() ?? "grpc") == "grpc"
                            ? OpenTelemetry.Exporter.OtlpExportProtocol.Grpc
                            : OpenTelemetry.Exporter.OtlpExportProtocol.HttpProtobuf;
                    });
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
            });
    }
}
