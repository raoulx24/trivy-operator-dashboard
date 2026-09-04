using k8s.Models;
using OpenTelemetry.Exporter;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using System.Reflection;
using TrivyOperator.Dashboard.Api.HealthChecks;
using TrivyOperator.Dashboard.Application.Alerts.Abstractions;
using TrivyOperator.Dashboard.Application.Alerts.Models;
using TrivyOperator.Dashboard.Application.GitHub.Options;
using TrivyOperator.Dashboard.Application.GitHub.Services;
using TrivyOperator.Dashboard.Application.History.VulnerabilityReportsHistory.Retention;
using TrivyOperator.Dashboard.Application.History.VulnerabilityReportsHistory.Services;
using TrivyOperator.Dashboard.Application.K8sEventPipeline.HostedServices;
using TrivyOperator.Dashboard.Application.K8sEventPipeline.Models.WatcherEvents;
using TrivyOperator.Dashboard.Application.K8sEventPipeline.Services.BackgroundQueues;
using TrivyOperator.Dashboard.Application.K8sEventPipeline.Services.BackgroundQueues.Abstractions;
using TrivyOperator.Dashboard.Application.K8sEventPipeline.Services.EventDispatchers;
using TrivyOperator.Dashboard.Application.K8sEventPipeline.Services.EventDispatchers.Abstractions;
using TrivyOperator.Dashboard.Application.K8sEventPipeline.Services.EventPipelineStarters;
using TrivyOperator.Dashboard.Application.K8sEventPipeline.Services.EventPipelineStarters.Abstractions;
using TrivyOperator.Dashboard.Application.K8sEventPipeline.Services.EventProcessors;
using TrivyOperator.Dashboard.Application.K8sEventPipeline.Services.EventProcessors.Abstractions;
using TrivyOperator.Dashboard.Application.K8sEventPipeline.Services.Options;
using TrivyOperator.Dashboard.Application.K8sEventPipeline.Services.Watchers;
using TrivyOperator.Dashboard.Application.K8sEventPipeline.Services.Watchers.Abstractions;
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
using TrivyOperator.Dashboard.Application.Queries.Trivy.Services.VulnerabilityReports;
using TrivyOperator.Dashboard.Application.Queries.Trivy.Services.VulnerabilityReports.Abstractions;
using TrivyOperator.Dashboard.Application.Queries.TrivyDependencies.Services;
using TrivyOperator.Dashboard.Application.Queries.TrivyDependencies.Services.Abstractions;
using TrivyOperator.Dashboard.Application.Queries.WatcherStates.Services;
using TrivyOperator.Dashboard.Application.Queries.WatcherStates.Services.Abstractions;
using TrivyOperator.Dashboard.Application.WatcherStates.HostedServices;
using TrivyOperator.Dashboard.Application.WatcherStates.Models;
using TrivyOperator.Dashboard.Application.WatcherStates.Services;
using TrivyOperator.Dashboard.Domain.History.VulnerabilityReportsHistory;
using TrivyOperator.Dashboard.Domain.History.VulnerabilityReportsHistory.Services;
using TrivyOperator.Dashboard.Domain.History.VulnerabilityReportsHistory.Services.Abstractions;
using TrivyOperator.Dashboard.Domain.History.VulnerabilityReportsHistory.Stores.Abstractions;
using TrivyOperator.Dashboard.Domain.K8s.Entities;
using TrivyOperator.Dashboard.Domain.K8s.ValueObjects;
using TrivyOperator.Dashboard.Domain.Shared.Stores.Abstractions;
using TrivyOperator.Dashboard.Domain.Trivy.Entities;
using TrivyOperator.Dashboard.Domain.Trivy.Entities.Abstracts;
using TrivyOperator.Dashboard.Domain.Trivy.ValueObjects.Shared;
using TrivyOperator.Dashboard.Infrastructure.BackgroundQueues;
using TrivyOperator.Dashboard.Infrastructure.Caching.ConcurrentCache;
using TrivyOperator.Dashboard.Infrastructure.Caching.ConcurrentCache.Abstractions;
using TrivyOperator.Dashboard.Infrastructure.Caching.Distributed;
using TrivyOperator.Dashboard.Infrastructure.Caching.Distributed.Client;
using TrivyOperator.Dashboard.Infrastructure.Caching.Distributed.Client.Abstractions;
using TrivyOperator.Dashboard.Infrastructure.Caching.InMemory;
using TrivyOperator.Dashboard.Infrastructure.Caching.InMemory.CacheEntries;
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
using TrivyOperator.Dashboard.Infrastructure.K8s.Mappers;
using TrivyOperator.Dashboard.Infrastructure.K8s.Mappers.Abstract;
using TrivyOperator.Dashboard.Infrastructure.K8s.Providers;
using TrivyOperator.Dashboard.Infrastructure.K8s.Services;
using TrivyOperator.Dashboard.Infrastructure.K8s.Services.Abstractions;
using TrivyOperator.Dashboard.Infrastructure.Persistence.Aggregators;
using TrivyOperator.Dashboard.Infrastructure.Persistence.Aggregators.Abstracts;
using TrivyOperator.Dashboard.Infrastructure.Persistence.CacheEntityCodec.Codecs;
using TrivyOperator.Dashboard.Infrastructure.Persistence.CacheEntityCodec.Codecs.Abstractions;
using TrivyOperator.Dashboard.Infrastructure.Persistence.K8s.Builders;
using TrivyOperator.Dashboard.Infrastructure.Persistence.K8s.Builders.Abstractions;
using TrivyOperator.Dashboard.Infrastructure.Persistence.Trivy.Builders;
using TrivyOperator.Dashboard.Infrastructure.StaticResources.Services;
using TrivyOperator.Dashboard.Infrastructure.Trivy.Factories;
using TrivyOperator.Dashboard.Infrastructure.Trivy.Mappers;
using TrivyOperator.Dashboard.Infrastructure.Trivy.Schema.ClusterComplianceReports;
using TrivyOperator.Dashboard.Infrastructure.Trivy.Schema.ConfigAuditReports;
using TrivyOperator.Dashboard.Infrastructure.Trivy.Schema.ExposedSecretReports;
using TrivyOperator.Dashboard.Infrastructure.Trivy.Schema.InfraAssessmentReports;
using TrivyOperator.Dashboard.Infrastructure.Trivy.Schema.RbacAssessmentReports;
using TrivyOperator.Dashboard.Infrastructure.Trivy.Schema.SbomReports;
using TrivyOperator.Dashboard.Infrastructure.Trivy.Schema.VulnerabilityReports.Models;

namespace TrivyOperator.Dashboard.Application.Common;

public static class BuilderServicesExtensions
{
    public static ILogger? Logger { get; set; }

    public static void AddNamespaceRelatedServices(this IServiceCollection services, IConfiguration configuration)
    {
        bool useStaticNamespaceService = LoadUseStaticNamespaceService(configuration);
        bool useFileRepository = LoadUseFileRepository(configuration);

        if (useFileRepository)
        {
            services.AddScoped<IKubernetesNamespaceService, KubernetesNamespaceNullService>();
            return;
        }
        
        // resource mapper
        services.AddSingleton<K8sNamespaceMapper>();
        services.AddSingleton<IResourceMapper<V1Namespace, K8sNamespace>>(sp =>
            sp.GetRequiredService<K8sNamespaceMapper>());
        services.AddSingleton<IResourceKeyProvider<V1Namespace, Uid>>(sp =>
            sp.GetRequiredService<K8sNamespaceMapper>());
        
        // -- cache entry builder
        services.AddSingleton<
            ICacheEntryBuilder<K8sNamespace, Uid>,
            K8sNamespaceCacheEntryBuilder>();
        
        // expiring cache
        services.AddSingleton<
            IExpiringResourceConcurrentDictionaryCache<Uid, CacheEntry<K8sNamespace, Uid>>,
            ExpiringResourceConcurrentDictionaryCache<Uid, CacheEntry<K8sNamespace, Uid>>>();
        
        // aggregator
        services.AddSingleton<
            IResourceAggregator<V1Namespace, K8sNamespace, Uid>, GenericResourceAggregator<V1Namespace, K8sNamespace>>();
        
        // expiring resource provider
        services.AddSingleton<
            IExpiringResourceProvider<K8sNamespace, Uid>,
            KubernetesResourceProvider<V1Namespace, K8sNamespace, Uid>>();

        // k8s services
        // -- k8s infra service
        if (useStaticNamespaceService)
        {
            services.AddSingleton<StaticNamespaceService>();

            services.AddSingleton<
                IClusterScopedResourceService<V1Namespace, V1NamespaceList>>(
                sp => sp.GetRequiredService<StaticNamespaceService>());

            services.AddSingleton<
                IKubernetesResourceService<V1Namespace>>(
                sp => sp.GetRequiredService<StaticNamespaceService>());
        }
        else
        {
            services.AddSingleton<NamespaceService>();

            services.AddSingleton<
                IClusterScopedResourceService<V1Namespace, V1NamespaceList>>(
                sp => sp.GetRequiredService<NamespaceService>());

            services.AddSingleton<
                IKubernetesResourceService<V1Namespace>>(
                sp => sp.GetRequiredService<NamespaceService>());
        }

        // -- k8s event pipeline starter
        services.AddSingleton<IKubernetesEventPipelineStarter, ClusterScopedEventPipelineStarter<V1Namespace>>();
        
        // -- watcher
        services.AddSingleton<IClusterScopedWatcher, ClusterScopedWatcher<V1NamespaceList, V1Namespace>>();
        
        // background queue
        services
            .AddSingleton<IKubernetesBackgroundQueue<V1Namespace>,
                KubernetesBackgroundQueue<V1Namespace>>();
        
        // k8s event dispatcher
        services.AddSingleton<IKubernetesEventDispatcher<V1Namespace>,
            KubernetesEventDispatcher<V1Namespace,
                IKubernetesBackgroundQueue<V1Namespace>>>();
        
        // processor for starting namespaced watchers
        services.AddSingleton<IKubernetesEventProcessor<V1Namespace>, NamespacedWatcherLifecycleProcessor>();
        
        
    }
    
    public static void AddTrivyReportRelatedServices(this IServiceCollection services, IConfiguration configuration)
    {
        bool useDefaultContext = LoadUseDefaultContext(configuration);
        bool useStaticNamespaceService = LoadUseStaticNamespaceService(configuration);
        Dictionary<string, bool> useTrivyReportServices = LoadEnabledTrivyReports(configuration);
        bool useFileRepository = LoadUseFileRepository(configuration);
        Dictionary<string, bool> useTrivyReportsInFileRepo = LoadTrivyReportsInFileRepo(configuration);
        
        services.AddSingleton<ICrdFactory, TrivyReportCrdFactory>();
        
        services.AddTrivyReport<VulnerabilityReportCr, VulnerabilityReport, Digest>();
        
        
    }
    
    public static void AddWatcherStateRelatedRelatedServices(this IServiceCollection services)
    {
        // IKubernetesEventProcessor<TKubernetesObject> is in AddTrivyReport
        services.AddSingleton<IConcurrentCache<WatcherKey, WatcherStateInfo>, ConcurrentCache<WatcherKey, WatcherStateInfo>>();
        services.AddScoped<IWatcherStatusService, WatcherStatusService>();
    }
    
    public static void AddHistoryRelatedServices(this IServiceCollection services, IConfiguration configuration)
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

    public static void AddKubernetesRelatedServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddHostedService<KubernetesEventPipelineHost>();
       
        services.AddSingleton<IKubernetesClientFactory, KubernetesClientFactory>();  

        if (configuration.GetSection("Kubernetes").GetValue<bool>("UseDefaultContext"))
        {
            services.AddSingleton<IKubernetesContextResolver, DefaultKubernetesContextResolver>();
        }
        else
        {
            services.AddSingleton<IKubernetesContextResolver, HttpHeaderKubernetesContextResolver>();  
        }
        
        services.AddSingleton<IKubernetesContextAccessor, KubernetesContextAccessor>();
        
        services.AddScoped<IKubernetesContextService, KubernetesContextService>();
    }

    public static void AddWatcherStateRelatedServices(this IServiceCollection services)
    {
        services.AddSingleton<
            IConcurrentCache<WatcherKey, WatcherStateInfo>, ConcurrentCache<WatcherKey, WatcherStateInfo>>();
        services.AddHostedService<WatcherStateCacheTimedHostedService>();
        
        // watcher state event processor
        // services
        //     .AddSingleton<IKubernetesEventProcessor<TReportCr>, 
        //         WatcherStateEventProcessor<TReportCr>>();
    }

    public static void AddGitHubRelatedServices(this IServiceCollection services, IConfiguration configuration)
    {
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
    }

    public static void AddTrivyDependenciesRelatedServices(this IServiceCollection services, IConfiguration configuration)
    {
        // add, above, null IProvider services for the vr, sbom, esr, car, if they are disabled
        services.AddScoped<ITrivyReportDependenciesService, TrivyReportDependenciesService>();
    }

    public static void AddMiscServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<IBackendSettingsService, BackendSettingsService>();
        
        services.AddHealthChecks()
            .AddCheck<WatchersLivenessHealthCheck>("watchers-liveness")
            .AddCheck<WatchersReadinessHealthCheck>("watchers-readiness");
        
    }

    public static void AddAppOptions(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<BackgroundQueueOptions>(configuration.GetSection("Queues"));
        services.Configure<KubernetesOptions>(configuration.GetSection("Kubernetes"));
        services.Configure<FileRepositoryOptions>(configuration.GetSection("FileRepository"));
        services.Configure<WatchersOptions>(configuration.GetSection("Watchers"));
        services.Configure<FileExportOptions>(configuration.GetSection("FileExport"));
        services.Configure<GitHubOptions>(configuration.GetSection("GitHub"));
    }
    
    public static void AddAlertsServices(this IServiceCollection services)
    {
        services.AddSignalR();
        services.AddSingleton<IConcurrentCache<AlertKey, Alert>, ConcurrentCache<AlertKey, Alert>>();
        services.AddSingleton<IAlertPublisher, AlertPublisher>();
        services.AddTransient<IAlertsService, AlertsService>();
    }

    public static void AddOpenTelemetry(
        this IServiceCollection services,
        IConfiguration configuration,
        string applicationName
    )
    {
        bool? a = configuration.GetValue<bool?>("Enabled");
        if (configuration.GetValue<bool?>("Enabled") ?? false)
        {
            services.AddSingleton<IMetricsClient>(_ => new MetricsClient(applicationName));

            // string fileVersion =
            //     Assembly.GetExecutingAssembly().GetCustomAttribute<AssemblyFileVersionAttribute>()?.Version ?? "0.0";
            // this is AOT friendly
            // TODO: verify build in github

            string fileVersion = Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "0.0";

            string? otelEndpoint = configuration.GetValue<string>("OtelEndpoint");
            bool? isConsoleEnabled = configuration.GetValue<bool?>("ConsoleEnabled");
            bool? isAspNetCoreEnabled = configuration.GetValue<bool?>("AspNetCoreInstrumentationEnabled");
            bool? isRuntimeEnabled = configuration.GetValue<bool?>("RuntimeInstrumentationEnabled");
            int? metricsPort = configuration.GetValue<int>("PrometheusExporterPort");
            double[] histogramBounds =
                configuration.GetValue<double[]>("HistogramBoundsInMs") ?? [200, 500, 1000, 5000,];

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
                                        (configuration.GetValue<string?>("OtelProtocol")?.ToLowerInvariant() ??
                                         "grpc") ==
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
                                        (configuration.GetValue<string?>("OTelProtocol")?.ToLowerInvariant() ??
                                         "grpc") ==
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
    }

    private static void AddTrivyReport<TReportCr, TReport, TId>(this IServiceCollection services)
    where TReportCr : CustomResource, new()
    where TReport : ITrivyReport<TId>
    where TId : notnull
    {
        // mapper service
        services.AddReportMapper(typeof(TReport));

        // in memory cache
        // -- codec
        // TODO: move it from here, there is no point in registering it multiple times
        services.AddSingleton<ICacheEntityCodec, BrotliMemoryPackCacheEntityCodec>();
        // -- cache entry builder
        services.AddCacheEntryBuilder(typeof(TReport));
            // services.AddSingleton<
            //     ICacheEntryBuilder<VulnerabilityReport, Digest>,
            //     VulnerabilityReportCacheEntryBuilder<VulnerabilityReport, Digest>>();
        // -- concurrent cache
        services
            .AddSingleton<IResourceConcurrentDictionaryCache<TId, CacheEntry<TReport, TId>>,
                ResourceConcurrentDictionaryCache<TId, CacheEntry<TReport, TId>>>();     
        // -- IResourceStore (in part) and IResourceProvider (out part)
        services.AddReportInMemoryCache(typeof(TReport));

        // k8s infra service, event pipeline starter, watcher
        services.AddReportKubernetesServices<TReportCr, TReport, TId>();
        
        // background queue
        services
            .AddSingleton<IKubernetesBackgroundQueue<TReportCr>,
                KubernetesBackgroundQueue<TReportCr>>();
        
        // k8s event dispatcher
        services.AddSingleton<IKubernetesEventDispatcher<TReportCr>,
            KubernetesEventDispatcher<TReportCr,
                IKubernetesBackgroundQueue<TReportCr>>>();
        
        // k8s event processor
        services
            .AddSingleton<IKubernetesEventProcessor<TReportCr>, 
                ResourceStoreUpdater<TReportCr,TReport,TId>>();
        
        // TODO: shall we keep it here? shall we keep it in AddWatcherStateRelatedServices?
        // // watcher state event processor
        // services
        //     .AddSingleton<IKubernetesEventProcessor<TReportCr>, 
        //         WatcherStateEventProcessor<TReportCr>>();

        services.AddScoped<IVulnerabilityReportService, VulnerabilityReportService>();
    }
    

    private static void AddCacheEntryBuilder(this IServiceCollection services, Type reportType)
    {
        switch (reportType.Name)
        {
            case nameof(ClusterComplianceReport):
                services.AddSingleton<
                    ICacheEntryBuilder<ClusterComplianceReport, Uid>,
                    ClusterComplianceReportCacheEntryBuilder>();
                break;

            case nameof(ClusterConfigAuditReport):
                services.AddSingleton<
                    ICacheEntryBuilder<ClusterConfigAuditReport, Uid>,
                    SecurityAssessmentReportCacheEntryBuilder<ClusterConfigAuditReport, Uid>>();
                break;

            case nameof(ClusterInfraAssessmentReport):
                services.AddSingleton<
                    ICacheEntryBuilder<ClusterInfraAssessmentReport, Uid>,
                    SecurityAssessmentReportCacheEntryBuilder<ClusterInfraAssessmentReport, Uid>>();
                break;

            case nameof(ClusterRbacAssessmentReport):
                services.AddSingleton<
                    ICacheEntryBuilder<ClusterRbacAssessmentReport, Uid>,
                    SecurityAssessmentReportCacheEntryBuilder<ClusterRbacAssessmentReport, Uid>>();
                break;

            case nameof(ClusterSbomReport):
                services.AddSingleton<
                    ICacheEntryBuilder<ClusterSbomReport, Uid>,
                    SbomReportCacheEntryBuilder<ClusterSbomReport, Uid>>();
                break;

            case nameof(ClusterVulnerabilityReport):
                services.AddSingleton<
                    ICacheEntryBuilder<ClusterVulnerabilityReport, Uid>,
                    VulnerabilityReportCacheEntryBuilder<ClusterVulnerabilityReport, Uid>>();
                break;

            case nameof(ConfigAuditReport):
                services.AddSingleton<
                    ICacheEntryBuilder<ConfigAuditReport, Uid>,
                    SecurityAssessmentReportCacheEntryBuilder<ConfigAuditReport, Uid>>();
                break;

            case nameof(ExposedSecretReport):
                services.AddSingleton<
                    ICacheEntryBuilder<ExposedSecretReport, Digest>,
                    ExposedSecretReportCacheEntryBuilder>();
                break;

            case nameof(InfraAssessmentReport):
                services.AddSingleton<
                    ICacheEntryBuilder<InfraAssessmentReport, Uid>,
                    SecurityAssessmentReportCacheEntryBuilder<InfraAssessmentReport, Uid>>();
                break;

            case nameof(RbacAssessmentReport):
                services.AddSingleton<
                    ICacheEntryBuilder<RbacAssessmentReport, Uid>,
                    SecurityAssessmentReportCacheEntryBuilder<RbacAssessmentReport, Uid>>();
                break;

            case nameof(SbomReport):
                services.AddSingleton<
                    ICacheEntryBuilder<SbomReport, Digest>,
                    SbomReportCacheEntryBuilder<SbomReport, Digest>>();
                break;

            case nameof(VulnerabilityReport):
                services.AddSingleton<
                    ICacheEntryBuilder<VulnerabilityReport, Digest>,
                    VulnerabilityReportCacheEntryBuilder<VulnerabilityReport, Digest>>();
                break;

            default:
                throw new NotSupportedException(
                    $"No cache entry builder registered for report type '{reportType.Name}'.");
        }
    }
    
    private static void AddReportMapper(this IServiceCollection services, Type reportType)
    {
        switch (reportType.Name)
        {
            case nameof(ClusterComplianceReport):
                services.AddSingleton<ClusterComplianceReportMapper>();
                services.AddSingleton<IResourceMapper<ClusterComplianceReportCr, ClusterComplianceReport>>(sp =>
                    sp.GetRequiredService<ClusterComplianceReportMapper>());
                services.AddSingleton<IResourceKeyProvider<ClusterComplianceReportCr, Uid>>(sp =>
                    sp.GetRequiredService<ClusterComplianceReportMapper>());
                break;

            case nameof(ClusterConfigAuditReport):
                services.AddSingleton<ClusterConfigAuditReportMapper>();
                services.AddSingleton<IResourceMapper<ClusterConfigAuditReportCr, ClusterConfigAuditReport>>(sp =>
                    sp.GetRequiredService<ClusterConfigAuditReportMapper>());
                services.AddSingleton<IResourceKeyProvider<ClusterConfigAuditReportCr, Uid>>(sp =>
                    sp.GetRequiredService<ClusterConfigAuditReportMapper>());
                break;

            case nameof(ClusterInfraAssessmentReport):
                services.AddSingleton<ClusterInfraAssessmentReportMapper>();
                services.AddSingleton<IResourceMapper<ClusterInfraAssessmentReportCr, ClusterInfraAssessmentReport>>(sp =>
                    sp.GetRequiredService<ClusterInfraAssessmentReportMapper>());
                services.AddSingleton<IResourceKeyProvider<ClusterInfraAssessmentReportCr, Uid>>(sp =>
                    sp.GetRequiredService<ClusterInfraAssessmentReportMapper>());
                break;

            case nameof(ClusterRbacAssessmentReport):
                services.AddSingleton<ClusterRbacAssessmentReportMapper>();
                services.AddSingleton<IResourceMapper<ClusterRbacAssessmentReportCr, ClusterRbacAssessmentReport>>(sp =>
                    sp.GetRequiredService<ClusterRbacAssessmentReportMapper>());
                services.AddSingleton<IResourceKeyProvider<ClusterRbacAssessmentReportCr, Uid>>(sp =>
                    sp.GetRequiredService<ClusterRbacAssessmentReportMapper>());
                break;

            case nameof(ClusterSbomReport):
                services.AddSingleton<ClusterSbomReportMapper>();
                services.AddSingleton<IResourceMapper<ClusterSbomReportCr, ClusterSbomReport>>(sp =>
                    sp.GetRequiredService<ClusterSbomReportMapper>());
                services.AddSingleton<IResourceKeyProvider<ClusterSbomReportCr, Uid>>(sp =>
                    sp.GetRequiredService<ClusterSbomReportMapper>());
                break;

            case nameof(ClusterVulnerabilityReport):
                services.AddSingleton<ClusterVulnerabilityReportMapper>();
                services.AddSingleton<IResourceMapper<ClusterVulnerabilityReportCr, ClusterVulnerabilityReport>>(sp =>
                    sp.GetRequiredService<ClusterVulnerabilityReportMapper>());
                services.AddSingleton<IResourceKeyProvider<ClusterVulnerabilityReportCr, Uid>>(sp =>
                    sp.GetRequiredService<ClusterVulnerabilityReportMapper>());
                break;

            case nameof(ConfigAuditReport):
                services.AddSingleton<ConfigAuditReportMapper>();
                services.AddSingleton<IResourceMapper<ConfigAuditReportCr, ConfigAuditReport>>(sp =>
                    sp.GetRequiredService<ConfigAuditReportMapper>());
                services.AddSingleton<IResourceKeyProvider<ConfigAuditReportCr, Uid>>(sp =>
                    sp.GetRequiredService<ConfigAuditReportMapper>());
                break;

            case nameof(ExposedSecretReport):
                services.AddSingleton<ExposedSecretReportMapper>();
                services.AddSingleton<IResourceMapper<ExposedSecretReportCr, ExposedSecretReport>>(sp =>
                    sp.GetRequiredService<ExposedSecretReportMapper>());
                services.AddSingleton<IResourceKeyProvider<ExposedSecretReportCr, Digest>>(sp =>
                    sp.GetRequiredService<ExposedSecretReportMapper>());
                break;

            case nameof(InfraAssessmentReport):
                services.AddSingleton<InfraAssessmentReportMapper>();
                services.AddSingleton<IResourceMapper<InfraAssessmentReportCr, InfraAssessmentReport>>(sp =>
                    sp.GetRequiredService<InfraAssessmentReportMapper>());
                services.AddSingleton<IResourceKeyProvider<InfraAssessmentReportCr, Uid>>(sp =>
                    sp.GetRequiredService<InfraAssessmentReportMapper>());
                break;

            case nameof(RbacAssessmentReport):
                services.AddSingleton<RbacAssessmentReportMapper>();
                services.AddSingleton<IResourceMapper<RbacAssessmentReportCr, RbacAssessmentReport>>(sp =>
                    sp.GetRequiredService<RbacAssessmentReportMapper>());
                services.AddSingleton<IResourceKeyProvider<RbacAssessmentReportCr, Uid>>(sp =>
                    sp.GetRequiredService<RbacAssessmentReportMapper>());
                break;

            case nameof(SbomReport):
                services.AddSingleton<SbomReportMapper>();
                services.AddSingleton<IResourceMapper<SbomReportCr, SbomReport>>(sp =>
                    sp.GetRequiredService<SbomReportMapper>());
                services.AddSingleton<IResourceKeyProvider<SbomReportCr, Digest>>(sp =>
                    sp.GetRequiredService<SbomReportMapper>());
                break;

            case nameof(VulnerabilityReport):
                services.AddSingleton<VulnerabilityReportMapper>();
                services.AddSingleton<IResourceMapper<VulnerabilityReportCr, VulnerabilityReport>>(sp =>
                    sp.GetRequiredService<VulnerabilityReportMapper>());
                services.AddSingleton<IResourceKeyProvider<VulnerabilityReportCr, Digest>>(sp =>
                    sp.GetRequiredService<VulnerabilityReportMapper>());
                break;

            default:
                throw new NotSupportedException(
                    $"No mapper registered for report type '{reportType.Name}'.");
        }
    }
    
    private static void AddReportInMemoryCache(this IServiceCollection services, Type reportType)
    {
        switch (reportType.Name)
        {
            case nameof(ClusterComplianceReport):
                services.AddSingleton<InMemoryResourceReportCache<ClusterComplianceReport>>();
                services.AddSingleton<IResourceStore<ClusterComplianceReport, Uid>>(sp =>
                    sp.GetRequiredService<InMemoryResourceReportCache<ClusterComplianceReport>>());
                services.AddSingleton<IResourceProvider<ClusterComplianceReport, Uid>>(sp =>
                    sp.GetRequiredService<InMemoryResourceReportCache<ClusterComplianceReport>>());
                break;

            case nameof(ClusterConfigAuditReport):
                services.AddSingleton<InMemoryResourceReportCache<ClusterConfigAuditReport>>();
                services.AddSingleton<IResourceStore<ClusterConfigAuditReport, Uid>>(sp =>
                    sp.GetRequiredService<InMemoryResourceReportCache<ClusterConfigAuditReport>>());
                services.AddSingleton<IResourceProvider<ClusterConfigAuditReport, Uid>>(sp =>
                    sp.GetRequiredService<InMemoryResourceReportCache<ClusterConfigAuditReport>>());
                break;

            case nameof(ClusterInfraAssessmentReport):
                services.AddSingleton<InMemoryResourceReportCache<ClusterInfraAssessmentReport>>();
                services.AddSingleton<IResourceStore<ClusterInfraAssessmentReport, Uid>>(sp =>
                    sp.GetRequiredService<InMemoryResourceReportCache<ClusterInfraAssessmentReport>>());
                services.AddSingleton<IResourceProvider<ClusterInfraAssessmentReport, Uid>>(sp =>
                    sp.GetRequiredService<InMemoryResourceReportCache<ClusterInfraAssessmentReport>>());
                break;

            case nameof(ClusterRbacAssessmentReport):
                services.AddSingleton<InMemoryResourceReportCache<ClusterRbacAssessmentReport>>();
                services.AddSingleton<IResourceStore<ClusterRbacAssessmentReport, Uid>>(sp =>
                    sp.GetRequiredService<InMemoryResourceReportCache<ClusterRbacAssessmentReport>>());
                services.AddSingleton<IResourceProvider<ClusterRbacAssessmentReport, Uid>>(sp =>
                    sp.GetRequiredService<InMemoryResourceReportCache<ClusterRbacAssessmentReport>>());
                break;

            case nameof(ClusterSbomReport):
                services.AddSingleton<InMemoryResourceReportCache<ClusterSbomReport>>();
                services.AddSingleton<IResourceStore<ClusterSbomReport, Uid>>(sp =>
                    sp.GetRequiredService<InMemoryResourceReportCache<ClusterSbomReport>>());
                services.AddSingleton<IResourceProvider<ClusterSbomReport, Uid>>(sp =>
                    sp.GetRequiredService<InMemoryResourceReportCache<ClusterSbomReport>>());
                break;

            case nameof(ClusterVulnerabilityReport):
                services.AddSingleton<InMemoryResourceReportCache<ClusterVulnerabilityReport>>();
                services.AddSingleton<IResourceStore<ClusterVulnerabilityReport, Uid>>(sp =>
                    sp.GetRequiredService<InMemoryResourceReportCache<ClusterVulnerabilityReport>>());
                services.AddSingleton<IResourceProvider<ClusterVulnerabilityReport, Uid>>(sp =>
                    sp.GetRequiredService<InMemoryResourceReportCache<ClusterVulnerabilityReport>>());
                break;

            case nameof(ConfigAuditReport):
                services.AddSingleton<InMemoryResourceReportCache<ConfigAuditReport>>();
                services.AddSingleton<IResourceStore<ConfigAuditReport, Uid>>(sp =>
                    sp.GetRequiredService<InMemoryResourceReportCache<ConfigAuditReport>>());
                services.AddSingleton<IResourceProvider<ConfigAuditReport, Uid>>(sp =>
                    sp.GetRequiredService<InMemoryResourceReportCache<ConfigAuditReport>>());
                break;

            case nameof(ExposedSecretReport):
                services.AddSingleton<InMemoryImageReportCache<ExposedSecretReport>>();
                services.AddSingleton<IResourceStore<ExposedSecretReport, Digest>>(sp =>
                    sp.GetRequiredService<InMemoryImageReportCache<ExposedSecretReport>>());
                services.AddSingleton<IResourceProvider<ExposedSecretReport, Digest>>(sp =>
                    sp.GetRequiredService<InMemoryImageReportCache<ExposedSecretReport>>());
                break;

            case nameof(InfraAssessmentReport):
                services.AddSingleton<InMemoryResourceReportCache<InfraAssessmentReport>>();
                services.AddSingleton<IResourceStore<InfraAssessmentReport, Uid>>(sp =>
                    sp.GetRequiredService<InMemoryResourceReportCache<InfraAssessmentReport>>());
                services.AddSingleton<IResourceProvider<InfraAssessmentReport, Uid>>(sp =>
                    sp.GetRequiredService<InMemoryResourceReportCache<InfraAssessmentReport>>());
                break;

            case nameof(RbacAssessmentReport):
                services.AddSingleton<InMemoryResourceReportCache<RbacAssessmentReport>>();
                services.AddSingleton<IResourceStore<RbacAssessmentReport, Uid>>(sp =>
                    sp.GetRequiredService<InMemoryResourceReportCache<RbacAssessmentReport>>());
                services.AddSingleton<IResourceProvider<RbacAssessmentReport, Uid>>(sp =>
                    sp.GetRequiredService<InMemoryResourceReportCache<RbacAssessmentReport>>());
                break;

            case nameof(SbomReport):
                services.AddSingleton<InMemoryImageReportCache<SbomReport>>();
                services.AddSingleton<IResourceStore<SbomReport, Digest>>(sp =>
                    sp.GetRequiredService<InMemoryImageReportCache<SbomReport>>());
                services.AddSingleton<IResourceProvider<SbomReport, Digest>>(sp =>
                    sp.GetRequiredService<InMemoryImageReportCache<SbomReport>>());
                break;

            case nameof(VulnerabilityReport):
                services.AddSingleton<InMemoryImageReportCache<VulnerabilityReport>>();
                services.AddSingleton<IResourceStore<VulnerabilityReport, Digest>>(sp =>
                    sp.GetRequiredService<InMemoryImageReportCache<VulnerabilityReport>>());
                services.AddSingleton<IResourceProvider<VulnerabilityReport, Digest>>(sp =>
                    sp.GetRequiredService<InMemoryImageReportCache<VulnerabilityReport>>());
                break;

            default:
                throw new NotSupportedException(
                    $"No report cache registered for report type '{reportType.Name}'.");
        }
    }

    private static void AddReportKubernetesServices<TReportCr, TReport, TId>(this IServiceCollection services)
        where TReportCr : CustomResource, new()
        where TReport : ITrivyReport<TId>
        where TId : notnull
    {
        if (nameof(TReport).StartsWith("Cluster", StringComparison.Ordinal))
        {
            // k8s infra service
            services
                .AddSingleton<
                    IClusterScopedResourceService<TReportCr, CustomResourceList<TReportCr>>,
                    ClusterScopedCustomResourceService<TReportCr>>();
        
            // k8s event pipeline starter
            services.AddSingleton<IKubernetesEventPipelineStarter, ClusterScopedEventPipelineStarter<TReportCr>>();
        
            // watcher
            services.AddSingleton<IClusterScopedWatcher, ClusterScopedWatcher<CustomResourceList<TReportCr>, TReportCr>>();
        }
        else
        {
            // k8s infra service
            services
                .AddSingleton<
                    INamespacedResourceService<TReportCr, CustomResourceList<TReportCr>>,
                    NamespacedCustomResourceService<TReportCr>>();
        
            // k8s event pipeline starter
            services.AddSingleton<IKubernetesEventPipelineStarter, NamespacedEventPipelineStarter<TReportCr>>();
            
            // watcher
            services.AddSingleton<INamespacedWatcher, NamespacedWatcher<CustomResourceList<TReportCr>, TReportCr>>();
        }
    }
    
    private static Dictionary<string, bool> LoadEnabledTrivyReports(IConfiguration config)
    {
        Dictionary<string, bool> useTrivyReportServices =
            new(StringComparer.OrdinalIgnoreCase);

        IConfigurationSection enabledTrivyReportsSection =
            config.GetSection("EnabledTrivyReports");

        foreach (IConfigurationSection kv in enabledTrivyReportsSection.GetChildren())
        {
            useTrivyReportServices[kv.Key] = kv.Get<bool>();
        }

        return useTrivyReportServices;
    }

    private static bool LoadUseDefaultContext(IConfiguration config)
    {
        return config.GetValue<bool>("Kubernetes:UseDefaultContext");
    }

    private static bool LoadUseFileRepository(IConfiguration config)
    {
        return !string.IsNullOrEmpty(
            config.GetValue<string>("FileRepository:BasePath"));
    }

    private static Dictionary<string, bool> LoadTrivyReportsInFileRepo(
        IConfiguration config)
    {
        Dictionary<string, bool> useTrivyReportsInFileRepo =
            new(StringComparer.OrdinalIgnoreCase);

        IConfigurationSection fileRepoSection =
            config.GetSection("FileRepository");

        foreach (IConfigurationSection kv in fileRepoSection.GetChildren())
        {
            if (kv.Key.EndsWith("Subpath", StringComparison.Ordinal))
            {
                string className = kv.Key[..^"Subpath".Length];

                useTrivyReportsInFileRepo[className] =
                    !string.IsNullOrEmpty(kv.Value);
            }
        }

        return useTrivyReportsInFileRepo;
    }

    private static bool LoadUseStaticNamespaceService(IConfiguration config)
    {
        string? namespaceList =
            config.GetValue<string>("Kubernetes:NamespaceList");

        return !string.IsNullOrWhiteSpace(namespaceList);
    }
}
