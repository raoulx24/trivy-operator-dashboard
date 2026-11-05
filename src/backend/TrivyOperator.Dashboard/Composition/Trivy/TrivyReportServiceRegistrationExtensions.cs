using TrivyOperator.Dashboard.Application.Kubernetes.EventPipeline.BackgroundQueues.Abstractions;
using TrivyOperator.Dashboard.Application.Kubernetes.EventPipeline.EventDispatchers;
using TrivyOperator.Dashboard.Application.Kubernetes.EventPipeline.EventDispatchers.Abstractions;
using TrivyOperator.Dashboard.Application.Kubernetes.EventPipeline.EventPipelineStarters;
using TrivyOperator.Dashboard.Application.Kubernetes.EventPipeline.EventProcessors;
using TrivyOperator.Dashboard.Application.Kubernetes.EventPipeline.EventProcessors.Abstractions;
using Microsoft.Extensions.Options;
using TrivyOperator.Dashboard.Application.Kubernetes.Contexts.Abstractions;
using TrivyOperator.Dashboard.Application.Kubernetes.WatcherRegistries.Abstractions;
using TrivyOperator.Dashboard.Application.Metrics.Abstractions;
using TrivyOperator.Dashboard.Application.Shared.EventPipeline.EventPipelineStarters.Abstractions;
using TrivyOperator.Dashboard.Application.Shared.Details.Abstractions;
using TrivyOperator.Dashboard.Application.Shared.Details.KubernetesEventProcessors;
using TrivyOperator.Dashboard.Application.Shared.Details.Options;
using TrivyOperator.Dashboard.Application.Shared.Details.Services;
using TrivyOperator.Dashboard.Application.Trivy.SecurityAssessmentDetails.KubernetesEventProcessors;
using TrivyOperator.Dashboard.Application.Trivy.SecurityAssessmentDetails.Providers;
using TrivyOperator.Dashboard.Application.Trivy.VulnerabilityDetails.KubernetesEventProcessors;
using TrivyOperator.Dashboard.Application.Trivy.VulnerabilityDetails.Providers;
using TrivyOperator.Dashboard.Application.Trivy.Queries.Severities.Services;
using TrivyOperator.Dashboard.Application.Trivy.Queries.Severities.Services.Abstractions;
using TrivyOperator.Dashboard.Application.Trivy.Queries.Trivy.Options;
using TrivyOperator.Dashboard.Application.Trivy.Queries.Trivy.Services.ClusterComplianceReports;
using TrivyOperator.Dashboard.Application.Trivy.Queries.Trivy.Services.ClusterComplianceReports.Abstractions;
using TrivyOperator.Dashboard.Application.Trivy.Queries.Trivy.Services.ClusterConfigAuditReports;
using TrivyOperator.Dashboard.Application.Trivy.Queries.Trivy.Services.ClusterConfigAuditReports.Abstractions;
using TrivyOperator.Dashboard.Application.Trivy.Queries.Trivy.Services.ClusterInfraAssessmentReports;
using TrivyOperator.Dashboard.Application.Trivy.Queries.Trivy.Services.ClusterInfraAssessmentReports.Abstractions;
using TrivyOperator.Dashboard.Application.Trivy.Queries.Trivy.Services.ClusterRbacAssessmentReports;
using TrivyOperator.Dashboard.Application.Trivy.Queries.Trivy.Services.ClusterRbacAssessmentReports.Abstractions;
using TrivyOperator.Dashboard.Application.Trivy.Queries.Trivy.Services.ClusterSbomReports;
using TrivyOperator.Dashboard.Application.Trivy.Queries.Trivy.Services.ClusterSbomReports.Abstractions;
using TrivyOperator.Dashboard.Application.Trivy.Queries.Trivy.Services.ClusterVulnerabilityReports;
using TrivyOperator.Dashboard.Application.Trivy.Queries.Trivy.Services.ClusterVulnerabilityReports.Abstractions;
using TrivyOperator.Dashboard.Application.Trivy.Queries.Trivy.Services.ConfigAuditReports;
using TrivyOperator.Dashboard.Application.Trivy.Queries.Trivy.Services.ConfigAuditReports.Abstractions;
using TrivyOperator.Dashboard.Application.Trivy.Queries.Trivy.Services.ExposedSecretReports;
using TrivyOperator.Dashboard.Application.Trivy.Queries.Trivy.Services.ExposedSecretReports.Abstractions;
using TrivyOperator.Dashboard.Application.Trivy.Queries.Trivy.Services.InfraAssessmentReports;
using TrivyOperator.Dashboard.Application.Trivy.Queries.Trivy.Services.InfraAssessmentReports.Abstractions;
using TrivyOperator.Dashboard.Application.Trivy.Queries.Trivy.Services.RbacAssessmentReports;
using TrivyOperator.Dashboard.Application.Trivy.Queries.Trivy.Services.RbacAssessmentReports.Abstractions;
using TrivyOperator.Dashboard.Application.Trivy.Queries.Trivy.Services.SbomReports;
using TrivyOperator.Dashboard.Application.Trivy.Queries.Trivy.Services.SbomReports.Abstractions;
using TrivyOperator.Dashboard.Application.Trivy.Queries.Trivy.Services.VulnerabilityReports;
using TrivyOperator.Dashboard.Application.Trivy.Queries.Trivy.Services.VulnerabilityReports.Abstractions;
using TrivyOperator.Dashboard.Composition.Configuration;
using TrivyOperator.Dashboard.Composition.Shared;
using TrivyOperator.Dashboard.Domain.Kubernetes.ValueObjects;
using TrivyOperator.Dashboard.Domain.Shared.Abstractions;
using TrivyOperator.Dashboard.Domain.Shared.Details;
using TrivyOperator.Dashboard.Domain.Shared.Stores;
using TrivyOperator.Dashboard.Domain.Shared.Stores.Abstractions;
using TrivyOperator.Dashboard.Domain.Trivy.Entities;
using TrivyOperator.Dashboard.Domain.Trivy.Entities.Abstracts;
using TrivyOperator.Dashboard.Domain.Trivy.ValueObjects.SecurityAssessments;
using TrivyOperator.Dashboard.Domain.Trivy.ValueObjects.Shared;
using TrivyOperator.Dashboard.Domain.Trivy.ValueObjects.Vulnerabilities;
using TrivyOperator.Dashboard.Infrastructure.Caching.CacheEntityCodec.Codecs;
using TrivyOperator.Dashboard.Infrastructure.Caching.CacheEntityCodec.Codecs.Abstractions;
using TrivyOperator.Dashboard.Infrastructure.Caching.ConcurrentCache;
using TrivyOperator.Dashboard.Infrastructure.Caching.ConcurrentCache.Abstractions;
using TrivyOperator.Dashboard.Infrastructure.Caching.InMemory.CacheEntries;
using TrivyOperator.Dashboard.Infrastructure.Caching.InMemory.Details;
using TrivyOperator.Dashboard.Infrastructure.Caching.InMemory.Details.SecurityAssessments;
using TrivyOperator.Dashboard.Infrastructure.Caching.InMemory.Details.Vulnerabilities;
using TrivyOperator.Dashboard.Infrastructure.Caching.InMemory.TrivyReports;
using TrivyOperator.Dashboard.Infrastructure.FileRepository.Abstractions;
using TrivyOperator.Dashboard.Infrastructure.FileRepository.Factories;
using TrivyOperator.Dashboard.Infrastructure.FileRepository.Services;
using TrivyOperator.Dashboard.Infrastructure.FileRepository.Services.Abstractions;
using TrivyOperator.Dashboard.Infrastructure.Kubernetes.CacheEntryBuilders;
using TrivyOperator.Dashboard.Infrastructure.Kubernetes.CacheEntryBuilders.Abstractions;
using TrivyOperator.Dashboard.Infrastructure.Kubernetes.CustomResources;
using TrivyOperator.Dashboard.Infrastructure.Kubernetes.EventPipeline.BackgroundQueues;
using TrivyOperator.Dashboard.Infrastructure.Kubernetes.EventPipeline.Services.EventPublishers;
using TrivyOperator.Dashboard.Infrastructure.Kubernetes.EventPipeline.Services.EventPublishers.Abstractions;
using TrivyOperator.Dashboard.Infrastructure.Kubernetes.EventPipeline.Services.ResourceWatches;
using TrivyOperator.Dashboard.Infrastructure.Kubernetes.EventPipeline.Services.ResourceWatches.Abstractions;
using TrivyOperator.Dashboard.Infrastructure.Kubernetes.EventPipeline.Services.WatcherRegistries;
using TrivyOperator.Dashboard.Infrastructure.Kubernetes.EventPipeline.Services.WatchSessions;
using TrivyOperator.Dashboard.Infrastructure.Kubernetes.EventPipeline.Services.WatchSessions.Abstractions;
using TrivyOperator.Dashboard.Infrastructure.Kubernetes.Mappers.Abstract;
using TrivyOperator.Dashboard.Infrastructure.Kubernetes.PersistenceAggregators;
using TrivyOperator.Dashboard.Infrastructure.Kubernetes.PersistenceAggregators.Abstracts;
using TrivyOperator.Dashboard.Infrastructure.Kubernetes.Providers;
using TrivyOperator.Dashboard.Infrastructure.Kubernetes.ResourceMaterializer;
using TrivyOperator.Dashboard.Infrastructure.Kubernetes.ResourceMaterializer.Abstractions;
using TrivyOperator.Dashboard.Infrastructure.Kubernetes.Services;
using TrivyOperator.Dashboard.Infrastructure.Kubernetes.Services.Abstractions;
using TrivyOperator.Dashboard.Infrastructure.Trivy.CacheEntryBuilders;
using TrivyOperator.Dashboard.Infrastructure.Trivy.Factories;
using TrivyOperator.Dashboard.Infrastructure.Trivy.Mappers.ToDomain;
using TrivyOperator.Dashboard.Infrastructure.Trivy.Schema.ClusterComplianceReports;
using TrivyOperator.Dashboard.Infrastructure.Trivy.Schema.ConfigAuditReports;
using TrivyOperator.Dashboard.Infrastructure.Trivy.Schema.ExposedSecretReports;
using TrivyOperator.Dashboard.Infrastructure.Trivy.Schema.InfraAssessmentReports;
using TrivyOperator.Dashboard.Infrastructure.Trivy.Schema.RbacAssessmentReports;
using TrivyOperator.Dashboard.Infrastructure.Trivy.Schema.SbomReports;
using TrivyOperator.Dashboard.Infrastructure.Trivy.Schema.VulnerabilityReports.Models;

namespace TrivyOperator.Dashboard.Composition.Trivy;

public static class TrivyReportServiceRegistrationExtensions
{
    // 1st level - main entrance
    public static void AddTrivyReportRelatedServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<FileExportOptions>(configuration.GetSection("FileExport"));

        services.AddOptions<DetailsOptions>().Bind(configuration.GetSection("Details"));

        services.AddSingleton<ICrdFactory, TrivyReportCrdFactory>();
        
        services.AddSingleton<ICacheEntityCodec, BrotliMemoryPackCacheEntityCodec>();
        
        if (TrivyDashboardConfigurationReader.LoadUseFileRepository(configuration))
            services.AddSingleton<IFolderNameFactory, FolderNameFactory>();

        services.AddScoped<ISeverityService, SeverityService>();

        services.AddTrivyReportServices<ClusterComplianceReportCr, ClusterComplianceReport, Uid>(configuration);
        
        services.AddTrivyReportServices<ClusterConfigAuditReportCr, ClusterConfigAuditReport, Uid>(configuration);

        services.AddTrivyReportServices<ClusterInfraAssessmentReportCr, ClusterInfraAssessmentReport, Uid>(configuration);

        services.AddTrivyReportServices<ClusterRbacAssessmentReportCr, ClusterRbacAssessmentReport, Uid>(configuration);

        services.AddTrivyReportServices<ClusterSbomReportCr, ClusterSbomReport, Uid>(configuration);

        services.AddTrivyReportServices<ClusterVulnerabilityReportCr, ClusterVulnerabilityReport, Uid>(configuration);

        services.AddTrivyReportServices<ConfigAuditReportCr, ConfigAuditReport, Uid>(configuration);

        services.AddTrivyReportServices<ExposedSecretReportCr, ExposedSecretReport, Digest>(configuration);

        services.AddTrivyReportServices<InfraAssessmentReportCr, InfraAssessmentReport, Uid>(configuration);

        services.AddTrivyReportServices<RbacAssessmentReportCr, RbacAssessmentReport, Uid>(configuration);

        services.AddTrivyReportServices<SbomReportCr, SbomReport, Digest>(configuration);

        services.AddTrivyReportServices<VulnerabilityReportCr, VulnerabilityReport, Digest>(configuration);
    }
    
    private static void AddTrivyReportServices<TReportCr, TReport, TId>(
        this IServiceCollection services,
        IConfiguration configuration)
        where TReportCr : CustomResource, new()
        where TReport : class, ITrivyReport<TReport, TId>
        where TId : notnull
    {
        TrivyReportCompositionMode state = TrivyReportCompositionResolver.Resolve<TReport>(configuration);

        switch (state)
        {
            case TrivyReportCompositionMode.DefaultContext:
                CompositionLogger.Logger?.LogInformation("Adding Trivy Report {trivyReport} related services for Default Context",  typeof(TReport).Name);
                services.AddTrivyReportDefaultContext<TReportCr, TReport, TId>();
                break;
            
            case TrivyReportCompositionMode.MultiContext:
                CompositionLogger.Logger?.LogInformation("Adding Trivy Report {trivyReport} related services for Multi-Context",  typeof(TReport).Name);
                services.AddTrivyReportMultiContext<TReportCr, TReport, TId>();
                break;
            
            case TrivyReportCompositionMode.FileRepository:
                CompositionLogger.Logger?.LogInformation("Adding Trivy Report {trivyReport} related services for File Repository",  typeof(TReport).Name);
                services.AddTrivyReportFileRepo<TReportCr, TReport, TId>();
                break;

            case TrivyReportCompositionMode.Disabled:
                CompositionLogger.Logger?.LogInformation("Trivy Report {trivyReport} related services are disabled", typeof(TReport).Name);
                services.AddTrivyQueryRelatedNullServices(typeof(TReport));
                // an empty provider for the features reading other reports (e.g. Trivy Dependencies); disabled covers
                // the report turned off in EnabledTrivyReports and the one without a subpath in the file repository
                services.AddSingleton<IResourceProvider<TReport, TId>, NullResourceRepository<TReport, TId>>();
                break;


            default:
                throw new ArgumentOutOfRangeException(nameof(state), state, null);
        }
    }
    
    // 2nd level - add trivy reports based on running state
    
    private static void AddTrivyReportDefaultContext<TReportCr, TReport, TId>(this IServiceCollection services)
        where TReportCr : CustomResource, new()
        where TReport : class, ITrivyReport<TReport, TId>
        where TId : notnull
    {
        // mapper service
        services.AddReportMapper(typeof(TReport));

        // in memory cache
        // -- codec - registered above
        // services.AddSingleton<ICacheEntityCodec, BrotliMemoryPackCacheEntityCodec>();
        // -- the security assessments and the vulnerability reports keep a slim report here;
        //    their shared data (checks, vulnerability definitions, package vulnerabilities) is in stores of its own
        bool isSlim = services.TryAddSecurityAssessmentSlimServices(typeof(TReport)) ||
                      services.TryAddVulnerabilitySlimServices(typeof(TReport));

        // -- cache entry builder
        if (!isSlim)
            services.AddCacheEntryBuilder(typeof(TReport));
        // services.AddSingleton<
        //     ICacheEntryBuilder<VulnerabilityReport, Digest>,
        //     VulnerabilityReportCacheEntryBuilder<VulnerabilityReport, Digest>>();
        // -- concurrent cache
        services.AddSingleton<
            IResourceDictionaryCache<TId, CacheEntry<TReport, TId>>,
            ResourceDictionaryCache<TId, CacheEntry<TReport, TId>>>();     
        // -- IResourceStore (in part) and IResourceProvider (out part)
        if (!isSlim)
            services.AddReportInMemoryCache(typeof(TReport));

        // kubernetes infra service, event pipeline starter, watcher
        services.AddReportKubernetesPipelineServices<TReportCr, TReport, TId>();
        
        // background queue
        services.AddSingleton<
            IKubernetesBackgroundQueue<TReport,TId>,
            KubernetesBackgroundQueue<TReport,TId>>();
        
        // kubernetes event dispatcher
        services.AddSingleton<
            IKubernetesEventDispatcher<TReport,TId>,
            KubernetesEventDispatcher<TReport,TId>>();
        
        // kubernetes event processors
        // -- resource store (the slim reports register theirs above)
        if (!isSlim)
            services.AddSingleton<
                IKubernetesEventProcessor<TReport,TId>,
                ResourceStoreUpdater<TReport,TId>>();
        // -- metrics
        services.AddSingleton<
            IKubernetesEventProcessor<TReport,TId>,
            EventPipelineMetricsProcessor<TReport,TId>>();
        
        // query service
        services.AddTrivyQueryRelatedServices(typeof(TReport));
    }
    
    private static void AddTrivyReportMultiContext<TReportCr, TReport, TId>(this IServiceCollection services)
        where TReportCr : CustomResource, new()
        where TReport : class, ITrivyReport<TReport, TId>
        where TId : notnull
    {
        // mapper service
        services.AddReportMapper(typeof(TReport));
        
        // aggregators
        services.AddAggregatorServices(typeof(TReport));
        
        // kubernetes infra services
        services.AddReportKubernetesInfraServices<TReportCr, TReport, TId>();

        // cache entry builder, expiring cache and provider
        // -- the vulnerability reports keep a slim report as the entry, like in the default context
        if (!services.TryAddVulnerabilitySlimEntryServices(typeof(TReport), isFileRepository: false))
        {
            services.AddCacheEntryBuilder(typeof(TReport));
            // -- the providers take the builder with its entry type; here the entry is the report itself
            services.AddSingleton<ICacheEntryBuilder<TReport, TReport, TId>>(sp =>
                sp.GetRequiredService<ICacheEntryBuilder<TReport, TId>>());

            // expiring cache
            services.AddSingleton<
                IExpiringResourceDictionaryCache<TId, CacheEntry<TReport, TId>>,
                ExpiringResourceDictionaryCache<TId, CacheEntry<TReport, TId>>>();

            // provider
            services.AddSingleton<
                IResourceProvider<TReport, TId>,
                KubernetesResourceProvider<TReportCr, TReport, TReport, TId>>();
        }

        // query service
        services.AddTrivyQueryRelatedServices(typeof(TReport));
    }
    
    private static void AddTrivyReportFileRepo<TReportCr, TReport, TId>(this IServiceCollection services)
        where TReportCr : CustomResource, new()
        where TReport : class, ITrivyReport<TReport, TId>
        where TId : notnull
    {
        // mapper service
        services.AddReportMapper(typeof(TReport));
        
        // aggregators
        services.AddAggregatorServices(typeof(TReport));
        
        services.AddSingleton<
            IFileTrivyReportService<TReport, TId>,
            FileTrivyReportService<TReportCr, TReport, TId>>();

        // cache entry builder, expiring cache and provider
        // -- the vulnerability reports keep a slim report as the entry, like in the default context
        if (!services.TryAddVulnerabilitySlimEntryServices(typeof(TReport), isFileRepository: true))
        {
            services.AddCacheEntryBuilder(typeof(TReport));
            // -- the providers take the builder with its entry type; here the entry is the report itself
            services.AddSingleton<ICacheEntryBuilder<TReport, TReport, TId>>(sp =>
                sp.GetRequiredService<ICacheEntryBuilder<TReport, TId>>());

            services.AddSingleton<
                IExpiringResourceDictionaryCache<TId, CacheEntry<TReport, TId>>,
                ExpiringResourceDictionaryCache<TId, CacheEntry<TReport, TId>>>();

            // provider
            services.AddSingleton<
                IResourceProvider<TReport, TId>,
                FileTrivyReportProvider<TReport, TReport, TId>>();
        }

        // query service
        services.AddTrivyQueryRelatedServices(typeof(TReport));
    }
    
    private static void AddTrivyQueryRelatedNullServices(this IServiceCollection services, Type reportType)
    {
        switch (reportType.Name)
        {
            case nameof(ClusterComplianceReport):
                services.AddScoped<IClusterComplianceReportService, ClusterComplianceReportNullService>();
                break;

            case nameof(ClusterConfigAuditReport):
                services.AddScoped<IClusterConfigAuditReportService, ClusterConfigAuditReportNullService>();
                break;

            case nameof(ClusterInfraAssessmentReport):
                services.AddScoped<IClusterInfraAssessmentReportService, ClusterInfraAssessmentReportNullService>();
                break;

            case nameof(ClusterRbacAssessmentReport):
                services.AddScoped<IClusterRbacAssessmentReportService, ClusterRbacAssessmentReportNullService>();
                break;

            case nameof(ClusterSbomReport):
                services.AddScoped<IClusterSbomReportService, ClusterSbomReportNullService>();
                break;

            case nameof(ClusterVulnerabilityReport):
                services.AddScoped<IClusterVulnerabilityReportService, ClusterVulnerabilityReportNullService>();
                break;

            case nameof(ConfigAuditReport):
                services.AddScoped<IConfigAuditReportService, ConfigAuditReportNullService>();
                break;

            case nameof(ExposedSecretReport):
                services.AddScoped<IExposedSecretReportService, ExposedSecretReportNullService>();
                break;

            case nameof(InfraAssessmentReport):
                services.AddScoped<IInfraAssessmentReportService, InfraAssessmentReportNullService>();
                break;

            case nameof(RbacAssessmentReport):
                services.AddScoped<IRbacAssessmentReportService, RbacAssessmentReportNullService>();
                break;

            case nameof(SbomReport):
                services.AddScoped<ISbomReportService, SbomReportNullService>();
                break;

            case nameof(VulnerabilityReport):
                services.AddScoped<IVulnerabilityReportService, VulnerabilityReportNullService>();
                break;
            default:
                throw new NotSupportedException(
                    $"No query report registered for report type '{reportType.Name}'.");
        }
    }
    
    // 3rd level - helpers

    // the security assessments keep a slim report in the report cache and the check data in a store of their own,
    // shared by the namespaced and the cluster scoped report types of the same family
    // returns false for the other report types
    private static bool TryAddSecurityAssessmentSlimServices(this IServiceCollection services, Type reportType)
    {
        switch (reportType.Name)
        {
            case nameof(ClusterConfigAuditReport):
                services.AddSecurityAssessmentSlimServices<ClusterConfigAuditReport, ClusterConfigAuditReportSlim>("ConfigAudit");
                return true;

            case nameof(ClusterInfraAssessmentReport):
                services.AddSecurityAssessmentSlimServices<ClusterInfraAssessmentReport, ClusterInfraAssessmentReportSlim>("InfraAssessment");
                return true;

            case nameof(ClusterRbacAssessmentReport):
                services.AddSecurityAssessmentSlimServices<ClusterRbacAssessmentReport, ClusterRbacAssessmentReportSlim>("RbacAssessment");
                return true;

            case nameof(ConfigAuditReport):
                services.AddSecurityAssessmentSlimServices<ConfigAuditReport, ConfigAuditReportSlim>("ConfigAudit");
                return true;

            case nameof(InfraAssessmentReport):
                services.AddSecurityAssessmentSlimServices<InfraAssessmentReport, InfraAssessmentReportSlim>("InfraAssessment");
                return true;

            case nameof(RbacAssessmentReport):
                services.AddSecurityAssessmentSlimServices<RbacAssessmentReport, RbacAssessmentReportSlim>("RbacAssessment");
                return true;

            default:
                return false;
        }
    }

    private static void AddSecurityAssessmentSlimServices<TReport, TSlim>(
        this IServiceCollection services,
        string family)
        where TReport : class, ISecurityAssessmentReport<TReport, Uid>, ISlimmable<TSlim>
        where TSlim : class, IResourceReport<TSlim>, ISecurityAssessmentSlimReport<TSlim, TReport, Uid>
    {
        services.AddSecurityAssessmentDetails(family);

        // slim report cache - IResourceStore (in part) and IResourceProvider (out part)
        services.AddSingleton<ICacheEntryBuilder<TSlim, Uid>, PassthroughCacheEntryBuilder<TSlim, Uid>>();
        services.AddSingleton<
            IResourceDictionaryCache<Uid, CacheEntry<TSlim, Uid>>,
            ResourceDictionaryCache<Uid, CacheEntry<TSlim, Uid>>>();
        services.AddSingleton<InMemoryResourceReportCache<TSlim>>();
        services.AddSingleton<IResourceStore<TSlim, Uid>>(sp =>
            sp.GetRequiredService<InMemoryResourceReportCache<TSlim>>());
        services.AddSingleton<IResourceProvider<TSlim, Uid>>(sp =>
            sp.GetRequiredService<InMemoryResourceReportCache<TSlim>>());

        // the full report, for the query services
        services.AddSingleton<IResourceProvider<TReport, Uid>>(sp =>
            new HydratingSecurityAssessmentProvider<TReport, TSlim>(
                sp.GetRequiredService<IResourceProvider<TSlim, Uid>>(),
                sp.GetRequiredKeyedService<InMemoryDetailRepository<Check, CheckId>>(family),
                sp.GetRequiredService<ILogger<HydratingSecurityAssessmentProvider<TReport, TSlim>>>()));

        // the checks referenced by the stored reports, for the housekeeping
        services.AddKeyedSingleton<IDetailReferenceProvider<CheckId>>(family, (sp, _) =>
            new InMemoryCheckReferenceProvider<TSlim, TReport>(
                sp.GetRequiredService<IResourceDictionaryCache<Uid, CacheEntry<TSlim, Uid>>>()));

        // kubernetes event processors
        // -- the checks go first, so they are in their store before the report is in its own
        services.AddSingleton<IKubernetesEventProcessor<TReport, Uid>>(sp =>
            new SecurityAssessmentDetailsCollector<TReport, Uid>(
                sp.GetRequiredKeyedService<InMemoryDetailRepository<Check, CheckId>>(family),
                sp.GetRequiredService<ILogger<SecurityAssessmentDetailsCollector<TReport, Uid>>>()));
        // -- slim report store
        services.AddSingleton<ResourceStoreUpdater<TSlim, Uid>>();
        services.AddSingleton<IKubernetesEventProcessor<TReport, Uid>>(sp =>
            new SlimProjectionProcessor<TReport, TSlim, Uid>(
                sp.GetRequiredService<ResourceStoreUpdater<TSlim, Uid>>()));
    }

    // the vulnerability reports keep a slim report in the report cache and the vulnerability data in stores of their own
    // (vulnerability definitions and package vulnerabilities), shared by the namespaced and the cluster scoped reports
    // returns false for the other report types
    private static bool TryAddVulnerabilitySlimServices(this IServiceCollection services, Type reportType)
    {
        const string family = DetailFamilies.Vulnerability;

        switch (reportType.Name)
        {
            case nameof(ClusterVulnerabilityReport):
                // slim report cache - IResourceStore (in part) and IResourceProvider (out part)
                services.AddSingleton<InMemoryResourceReportCache<ClusterVulnerabilityReportSlim>>();
                services.AddSingleton<IResourceStore<ClusterVulnerabilityReportSlim, Uid>>(sp =>
                    sp.GetRequiredService<InMemoryResourceReportCache<ClusterVulnerabilityReportSlim>>());
                services.AddSingleton<IResourceProvider<ClusterVulnerabilityReportSlim, Uid>>(sp =>
                    sp.GetRequiredService<InMemoryResourceReportCache<ClusterVulnerabilityReportSlim>>());

                services.AddVulnerabilitySlimServices<ClusterVulnerabilityReport, ClusterVulnerabilityReportSlim, Uid>(family);
                return true;

            case nameof(VulnerabilityReport):
                // slim report cache - IResourceStore (in part) and IResourceProvider (out part)
                services.AddSingleton<InMemoryImageReportCache<VulnerabilityReportSlim>>();
                services.AddSingleton<IResourceStore<VulnerabilityReportSlim, Digest>>(sp =>
                    sp.GetRequiredService<InMemoryImageReportCache<VulnerabilityReportSlim>>());
                services.AddSingleton<IResourceProvider<VulnerabilityReportSlim, Digest>>(sp =>
                    sp.GetRequiredService<InMemoryImageReportCache<VulnerabilityReportSlim>>());

                services.AddVulnerabilitySlimServices<VulnerabilityReport, VulnerabilityReportSlim, Digest>(family);
                return true;

            default:
                return false;
        }
    }

    // multi-context and file repository: the vulnerability reports keep a slim report as the cache entry (as in the
    // default context), with the shared vulnerability data of the report encoded next to it
    // returns false for the other report types
    private static bool TryAddVulnerabilitySlimEntryServices(
        this IServiceCollection services,
        Type reportType,
        bool isFileRepository)
    {
        switch (reportType.Name)
        {
            case nameof(ClusterVulnerabilityReport):
                services.AddVulnerabilitySlimEntryServices<
                    ClusterVulnerabilityReportCr, ClusterVulnerabilityReport, ClusterVulnerabilityReportSlim, Uid>(
                    isFileRepository);
                return true;

            case nameof(VulnerabilityReport):
                services.AddVulnerabilitySlimEntryServices<
                    VulnerabilityReportCr, VulnerabilityReport, VulnerabilityReportSlim, Digest>(
                    isFileRepository);
                return true;

            default:
                return false;
        }
    }

    private static void AddVulnerabilitySlimEntryServices<TReportCr, TReport, TSlim, TId>(
        this IServiceCollection services,
        bool isFileRepository)
        where TReportCr : CustomResource
        where TReport : class, IVulnerabilityReport<TReport, TId>, ISlimmable<TSlim>
        where TSlim : class, IVulnerabilitySlimReport<TSlim, TReport, TId>
        where TId : notnull
    {
        services.AddSingleton<
            ICacheEntryBuilder<TReport, TSlim, TId>,
            VulnerabilityReportSlimCacheEntryBuilder<TReport, TSlim, TId>>();

        services.AddSingleton<
            IExpiringResourceDictionaryCache<TId, CacheEntry<TSlim, TId>>,
            ExpiringResourceDictionaryCache<TId, CacheEntry<TSlim, TId>>>();

        // the provider gives the reports (IResourceProvider) and the slim reports as stored (IResourceEntryProvider, for NVD)
        if (isFileRepository)
        {
            services.AddSingleton<FileTrivyReportProvider<TReport, TSlim, TId>>();
            services.AddSingleton<IResourceProvider<TReport, TId>>(sp =>
                sp.GetRequiredService<FileTrivyReportProvider<TReport, TSlim, TId>>());
            services.AddSingleton<IResourceEntryProvider<TSlim, TId>>(sp =>
                sp.GetRequiredService<FileTrivyReportProvider<TReport, TSlim, TId>>());
        }
        else
        {
            services.AddSingleton<KubernetesResourceProvider<TReportCr, TReport, TSlim, TId>>();
            services.AddSingleton<IResourceProvider<TReport, TId>>(sp =>
                sp.GetRequiredService<KubernetesResourceProvider<TReportCr, TReport, TSlim, TId>>());
            services.AddSingleton<IResourceEntryProvider<TSlim, TId>>(sp =>
                sp.GetRequiredService<KubernetesResourceProvider<TReportCr, TReport, TSlim, TId>>());
        }
    }

    private static void AddVulnerabilitySlimServices<TReport, TSlim, TId>(
        this IServiceCollection services,
        string family)
        where TReport : class, IVulnerabilityReport<TReport, TId>, ISlimmable<TSlim>
        where TSlim : class, IVulnerabilitySlimReport<TSlim, TReport, TId>
        where TId : notnull
    {
        services.AddDetails<VulnerabilityDefinition, VulnerabilityId>(family);
        services.AddDetails<PackageVulnerability, PackageVulnerabilityKey>(family);

        // slim report cache (the store and the provider are added by the caller, they depend on the key)
        services.AddSingleton<ICacheEntryBuilder<TSlim, TId>, PassthroughCacheEntryBuilder<TSlim, TId>>();
        services.AddSingleton<
            IResourceDictionaryCache<TId, CacheEntry<TSlim, TId>>,
            ResourceDictionaryCache<TId, CacheEntry<TSlim, TId>>>();

        // the full report, for the query services
        services.AddSingleton<IResourceProvider<TReport, TId>>(sp =>
            new HydratingVulnerabilityReportProvider<TReport, TSlim, TId>(
                sp.GetRequiredService<IResourceProvider<TSlim, TId>>(),
                sp.GetRequiredKeyedService<InMemoryDetailRepository<VulnerabilityDefinition, VulnerabilityId>>(family),
                sp.GetRequiredKeyedService<InMemoryDetailRepository<PackageVulnerability, PackageVulnerabilityKey>>(family),
                sp.GetRequiredService<ILogger<HydratingVulnerabilityReportProvider<TReport, TSlim, TId>>>()));

        // the details referenced by the stored reports, for the housekeeping
        services.AddKeyedSingleton<IDetailReferenceProvider<VulnerabilityId>>(family, (sp, _) =>
            new InMemoryVulnerabilityDefinitionReferenceProvider<TSlim, TReport, TId>(
                sp.GetRequiredService<IResourceDictionaryCache<TId, CacheEntry<TSlim, TId>>>()));
        services.AddKeyedSingleton<IDetailReferenceProvider<PackageVulnerabilityKey>>(family, (sp, _) =>
            new InMemoryPackageVulnerabilityReferenceProvider<TSlim, TReport, TId>(
                sp.GetRequiredService<IResourceDictionaryCache<TId, CacheEntry<TSlim, TId>>>()));

        // kubernetes event processors
        // -- the details go first, so they are in their stores before the report is in its own
        services.AddSingleton<IKubernetesEventProcessor<TReport, TId>>(sp =>
            new VulnerabilityDetailsCollector<TReport, TId>(
                sp.GetRequiredKeyedService<InMemoryDetailRepository<VulnerabilityDefinition, VulnerabilityId>>(family),
                sp.GetRequiredKeyedService<InMemoryDetailRepository<PackageVulnerability, PackageVulnerabilityKey>>(family),
                sp.GetRequiredService<ILogger<VulnerabilityDetailsCollector<TReport, TId>>>()));
        // -- slim report store
        services.AddSingleton<ResourceStoreUpdater<TSlim, TId>>();
        services.AddSingleton<IKubernetesEventProcessor<TReport, TId>>(sp =>
            new SlimProjectionProcessor<TReport, TSlim, TId>(
                sp.GetRequiredService<ResourceStoreUpdater<TSlim, TId>>()));
    }

    // one checks store and one housekeeping per family, added once
    private static void AddSecurityAssessmentDetails(this IServiceCollection services, string family)
        => services.AddDetails<Check, CheckId>(family);

    // one details store and one housekeeping per family and detail type, added once
    private static void AddDetails<TDetail, TId>(this IServiceCollection services, string family)
        where TDetail : class, IDetail<TId>
        where TId : notnull
    {
        bool isAdded = services.Any(x =>
            x.ServiceType == typeof(InMemoryDetailRepository<TDetail, TId>) &&
            x.ServiceKey is string key && key == family);

        if (isAdded)
        {
            return;
        }

        // the concurrent cache behind the store, keyed as well, for readers outside the family (e.g. the CVE subjects)
        services.AddKeyedSingleton<IResourceDictionaryCache<TId, DetailEntry<TDetail>>>(family, (sp, _) =>
            new ResourceDictionaryCache<TId, DetailEntry<TDetail>>(sp.GetRequiredService<IMetricsClient>()));
        services.AddKeyedSingleton<InMemoryDetailRepository<TDetail, TId>>(family, (sp, _) =>
            new InMemoryDetailRepository<TDetail, TId>(
                sp.GetRequiredKeyedService<IResourceDictionaryCache<TId, DetailEntry<TDetail>>>(family),
                sp.GetRequiredService<IKubernetesContextResolver>(),
                sp.GetRequiredService<IOptionsMonitor<DetailsOptions>>(),
                sp.GetRequiredService<ILogger<InMemoryDetailRepository<TDetail, TId>>>()));

        services.AddHostedService(sp =>
            new DetailsHousekeepingHostedService<TDetail, TId>(
                $"{family} {typeof(TDetail).Name}",
                sp.GetRequiredService<IOptionsMonitor<DetailsOptions>>(),
                sp.GetRequiredKeyedService<InMemoryDetailRepository<TDetail, TId>>(family),
                [.. sp.GetKeyedServices<IDetailReferenceProvider<TId>>(family),],
                sp.GetRequiredService<ILogger<DetailsHousekeepingHostedService<TDetail, TId>>>()));
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
    
    private static void AddReportKubernetesPipelineServices<TReportCr, TReport, TId>(this IServiceCollection services)
        where TReportCr : CustomResource, new()
        where TReport : class, ITrivyReport<TReport, TId>
        where TId : notnull
    {
        switch (typeof(TReport).Name)
        {
            case nameof(ClusterComplianceReport):
            case nameof(ClusterConfigAuditReport):
            case nameof(ClusterInfraAssessmentReport):
            case nameof(ClusterRbacAssessmentReport):
            case nameof(ClusterSbomReport):
            case nameof(ClusterVulnerabilityReport):
                
                // kubernetes services
                services.AddSingleton<ClusterScopedCustomResourceService<TReportCr>>();
                services.AddSingleton<
                    IClusterScopedResourceService<TReportCr, CustomResourceList<TReportCr>>>(
                    sp => sp.GetRequiredService<ClusterScopedCustomResourceService<TReportCr>>());
                services.AddSingleton<
                    IKubernetesResourceService<TReportCr>>(
                    sp => sp.GetRequiredService<ClusterScopedCustomResourceService<TReportCr>>());
                
                // kubernetes event pipeline starter
                services.AddSingleton<IEventPipelineStarter, ClusterScopedKubernetesEventPipelineStarter<TReport,TId>>();

                // kubernetes resource watcher
                services.AddSingleton<
                    IKubernetesResourceWatch<CustomResourceList<TReportCr>, TReportCr>,
                    ClusterScopedResourceWatch<CustomResourceList<TReportCr>, TReportCr>>();

                // resource materializer
                services.AddSingleton<
                    IResourceMaterializer<TReportCr, TReport, TId>,
                    ResourceMaterializer<TReportCr, TReport, TId>>();

                // kubernetes event publisher
                services.AddSingleton<IKubernetesEventPublisher<TReportCr>,KubernetesEventPublisher<TReportCr, TReport,TId>>();

                // kubernetes watch session factory
                services.AddSingleton<
                    IKubernetesWatchSessionFactory<CustomResourceList<TReportCr>, TReportCr>,
                    KubernetesWatchSessionFactory<CustomResourceList<TReportCr>, TReportCr>>();

                // kubernetes watcher registry
                services
                    .AddSingleton<ClusterScopedWatcherRegistry<CustomResourceList<TReportCr>, TReportCr, TReport, TId>>();
                services.AddSingleton<IClusterScopedWatcherRegistry>(
                    sp => sp.GetRequiredService<ClusterScopedWatcherRegistry<CustomResourceList<TReportCr>, TReportCr, TReport, TId>>());
                services.AddSingleton<IClusterScopedWatcherRegistry<TReport, TId>>(
                    sp => sp.GetRequiredService<ClusterScopedWatcherRegistry<CustomResourceList<TReportCr>, TReportCr, TReport, TId>>());
                
                break;

            case nameof(ConfigAuditReport):
            case nameof(ExposedSecretReport):
            case nameof(InfraAssessmentReport):
            case nameof(RbacAssessmentReport):
            case nameof(SbomReport):
            case nameof(VulnerabilityReport):
                
                // kubernetes services
                services.AddSingleton<NamespacedCustomResourceService<TReportCr>>();

                services.AddSingleton<
                    INamespacedResourceService<TReportCr, CustomResourceList<TReportCr>>>(
                    sp => sp.GetRequiredService<NamespacedCustomResourceService<TReportCr>>());

                services.AddSingleton<
                    IKubernetesResourceService<TReportCr>>(
                    sp => sp.GetRequiredService<NamespacedCustomResourceService<TReportCr>>());
                
                // kubernetes event pipeline starter
                services.AddSingleton<IEventPipelineStarter, NamespacedKubernetesEventPipelineStarter<TReport,TId>>();
            
                // kubernetes resource watcher
                services.AddSingleton<
                    IKubernetesResourceWatch<CustomResourceList<TReportCr>, TReportCr>,
                    NamespacedResourceWatch<CustomResourceList<TReportCr>, TReportCr>>();
                
                // resource materializer
                services.AddSingleton<
                    IResourceMaterializer<TReportCr, TReport, TId>,
                    ResourceMaterializer<TReportCr, TReport, TId>>();

                // kubernetes event publisher
                services.AddSingleton<IKubernetesEventPublisher<TReportCr>,KubernetesEventPublisher<TReportCr, TReport,TId>>();

                // kubernetes watch session factory
                services.AddSingleton<
                    IKubernetesWatchSessionFactory<CustomResourceList<TReportCr>, TReportCr>,
                    KubernetesWatchSessionFactory<CustomResourceList<TReportCr>, TReportCr>>();

                // kubernetes watcher registry
                services.AddSingleton<
                    INamespacedWatcherRegistry,
                    NamespacedWatcherRegistry<CustomResourceList<TReportCr>, TReportCr>>();
                
                break;

            default:
                throw new NotSupportedException(
                    $"No Kubernetes composition registered for report type '{typeof(TReport).Name}'.");

        }
    }
    
    private static void AddTrivyQueryRelatedServices(this IServiceCollection services, Type reportType)
    {
        switch (reportType.Name)
        {
            case nameof(ClusterComplianceReport):
                services.AddScoped<IClusterComplianceReportService, ClusterComplianceReportService>();
                break;

            case nameof(ClusterConfigAuditReport):
                services.AddScoped<IClusterConfigAuditReportService, ClusterConfigAuditReportService>();
                break;

            case nameof(ClusterInfraAssessmentReport):
                services.AddScoped<IClusterInfraAssessmentReportService, ClusterInfraAssessmentReportService>();
                break;

            case nameof(ClusterRbacAssessmentReport):
                services.AddScoped<IClusterRbacAssessmentReportService, ClusterRbacAssessmentReportService>();
                break;

            case nameof(ClusterSbomReport):
                services.AddScoped<IClusterSbomReportService, ClusterSbomReportService>();
                break;

            case nameof(ClusterVulnerabilityReport):
                services.AddScoped<IClusterVulnerabilityReportService, ClusterVulnerabilityReportService>();
                break;

            case nameof(ConfigAuditReport):
                services.AddScoped<IConfigAuditReportService, ConfigAuditReportService>();
                break;

            case nameof(ExposedSecretReport):
                services.AddScoped<IExposedSecretReportService, ExposedSecretReportService>();
                break;

            case nameof(InfraAssessmentReport):
                services.AddScoped<IInfraAssessmentReportService, InfraAssessmentReportService>();
                break;

            case nameof(RbacAssessmentReport):
                services.AddScoped<IRbacAssessmentReportService, RbacAssessmentReportService>();
                break;

            case nameof(SbomReport):
                services.AddScoped<ISbomReportService, SbomReportService>();
                break;

            case nameof(VulnerabilityReport):
                services.AddScoped<IVulnerabilityReportService, VulnerabilityReportService>();
                break;
            default:
                throw new NotSupportedException(
                    $"No query report registered for report type '{reportType.Name}'.");
        }
    }
    
    private static void AddAggregatorServices(this IServiceCollection services, Type reportType)
    {
        switch (reportType.Name)
        {
            case nameof(ClusterComplianceReport):
                services.AddSingleton<
                    IResourceAggregator<ClusterComplianceReportCr, ClusterComplianceReport, Uid>,
                    UidKeyedResourceAggregator<ClusterComplianceReportCr, ClusterComplianceReport>>();
                break;
            case nameof(ClusterConfigAuditReport):
                services.AddSingleton<
                    IResourceAggregator<ClusterConfigAuditReportCr, ClusterConfigAuditReport, Uid>,
                    UidKeyedResourceAggregator<ClusterConfigAuditReportCr, ClusterConfigAuditReport>>();
                break;
            case nameof(ClusterInfraAssessmentReport):
                services.AddSingleton<
                    IResourceAggregator<ClusterInfraAssessmentReportCr, ClusterInfraAssessmentReport, Uid>,
                    UidKeyedResourceAggregator<ClusterInfraAssessmentReportCr, ClusterInfraAssessmentReport>>();
                break;
            case nameof(ClusterRbacAssessmentReport):
                services.AddSingleton<
                    IResourceAggregator<ClusterRbacAssessmentReportCr, ClusterRbacAssessmentReport, Uid>,
                    UidKeyedResourceAggregator<ClusterRbacAssessmentReportCr, ClusterRbacAssessmentReport>>();
                break;
            case nameof(ClusterSbomReport):
                services.AddSingleton<
                    IResourceAggregator<ClusterSbomReportCr, ClusterSbomReport, Uid>,
                    UidKeyedResourceAggregator<ClusterSbomReportCr, ClusterSbomReport>>();
                break;
            case nameof(ClusterVulnerabilityReport):
                services.AddSingleton<
                    IResourceAggregator<ClusterVulnerabilityReportCr, ClusterVulnerabilityReport, Uid>,
                    UidKeyedResourceAggregator<ClusterVulnerabilityReportCr, ClusterVulnerabilityReport>>();
                break;
            case nameof(ConfigAuditReport):
                services.AddSingleton<
                    IResourceAggregator<ConfigAuditReportCr, ConfigAuditReport, Uid>,
                    UidKeyedResourceAggregator<ConfigAuditReportCr, ConfigAuditReport>>();
                break;
            case nameof(ExposedSecretReport):
                services.AddSingleton<
                    IResourceAggregator<ExposedSecretReportCr, ExposedSecretReport, Digest>,
                    DigestKeyedReportAggregator<ExposedSecretReportCr, ExposedSecretReport>>();
                break;
            case nameof(InfraAssessmentReport):
                services.AddSingleton<
                    IResourceAggregator<InfraAssessmentReportCr, InfraAssessmentReport, Uid>,
                    UidKeyedResourceAggregator<InfraAssessmentReportCr, InfraAssessmentReport>>();
                break;
            case nameof(RbacAssessmentReport):
                services.AddSingleton<
                    IResourceAggregator<RbacAssessmentReportCr, RbacAssessmentReport, Uid>,
                    UidKeyedResourceAggregator<RbacAssessmentReportCr, RbacAssessmentReport>>();
                break;
            case nameof(SbomReport):
                services.AddSingleton<
                    IResourceAggregator<SbomReportCr, SbomReport, Digest>,
                    DigestKeyedReportAggregator<SbomReportCr, SbomReport>>();
                break;
            case nameof(VulnerabilityReport):
                services.AddSingleton<
                    IResourceAggregator<VulnerabilityReportCr, VulnerabilityReport, Digest>,
                    DigestKeyedReportAggregator<VulnerabilityReportCr, VulnerabilityReport>>();
                break;

            default:
                throw new NotSupportedException(
                    $"No query report registered for report type '{reportType.Name}'.");
        }
    }
    
    private static void AddReportKubernetesInfraServices<TReportCr, TReport, TId>(this IServiceCollection services)
        where TReportCr : CustomResource, new()
        where TReport : ITrivyReport<TReport, TId>
        where TId : notnull
    {
        switch (typeof(TReport).Name)
        {
            case nameof(ClusterComplianceReport):
            case nameof(ClusterConfigAuditReport):
            case nameof(ClusterInfraAssessmentReport):
            case nameof(ClusterRbacAssessmentReport):
            case nameof(ClusterSbomReport):
            case nameof(ClusterVulnerabilityReport):
                
                // kubernetes services
                services.AddSingleton<ClusterScopedCustomResourceService<TReportCr>>();
                services.AddSingleton<
                    IClusterScopedResourceService<TReportCr, CustomResourceList<TReportCr>>>(
                    sp => sp.GetRequiredService<ClusterScopedCustomResourceService<TReportCr>>());
                services.AddSingleton<
                    IKubernetesResourceService<TReportCr>>(
                    sp => sp.GetRequiredService<ClusterScopedCustomResourceService<TReportCr>>());
                
                break;

            case nameof(ConfigAuditReport):
            case nameof(ExposedSecretReport):
            case nameof(InfraAssessmentReport):
            case nameof(RbacAssessmentReport):
            case nameof(SbomReport):
            case nameof(VulnerabilityReport):
                
                // kubernetes services
                services.AddSingleton<NamespacedCustomResourceService<TReportCr>>();

                services.AddSingleton<
                    INamespacedResourceService<TReportCr, CustomResourceList<TReportCr>>>(
                    sp => sp.GetRequiredService<NamespacedCustomResourceService<TReportCr>>());

                services.AddSingleton<
                    IKubernetesResourceService<TReportCr>>(
                    sp => sp.GetRequiredService<NamespacedCustomResourceService<TReportCr>>());
                
                break;

            default:
                throw new NotSupportedException(
                    $"No Kubernetes composition registered for report type '{typeof(TReport).Name}'.");

        }
    }
    
    // TODO: maybe use this style
    private static void Other_AddReportKubernetesPipelineServices<TReportCr, TReport, TId>(IServiceCollection services)
        where TReportCr : CustomResource, new()
        where TReport : class, ITrivyReport<TReport, TId>
        where TId : notnull
    {
        if (typeof(IClusterScopedTrivyReport).IsAssignableFrom(typeof(TReport)))
        {
            // AddClusterScopedReportPipeline<TReportCr>(services);
        }
        else if (typeof(INamespacedTrivyReport).IsAssignableFrom(typeof(TReport)))
        {
            // AddNamespacedReportPipeline<TReportCr>(services);
        }
        else
        {
            throw new NotSupportedException(
                $"Report type '{typeof(TReport).Name}' does not declare its Kubernetes scope.");
        }
    }
}
