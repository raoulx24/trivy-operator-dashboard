using TrivyOperator.Dashboard.Application.K8sEventPipeline.Services.BackgroundQueues;
using TrivyOperator.Dashboard.Application.K8sEventPipeline.Services.BackgroundQueues.Abstractions;
using TrivyOperator.Dashboard.Application.K8sEventPipeline.Services.EventDispatchers;
using TrivyOperator.Dashboard.Application.K8sEventPipeline.Services.EventDispatchers.Abstractions;
using TrivyOperator.Dashboard.Application.K8sEventPipeline.Services.EventPipelineStarters;
using TrivyOperator.Dashboard.Application.K8sEventPipeline.Services.EventPipelineStarters.Abstractions;
using TrivyOperator.Dashboard.Application.K8sEventPipeline.Services.EventProcessors;
using TrivyOperator.Dashboard.Application.K8sEventPipeline.Services.EventProcessors.Abstractions;
using TrivyOperator.Dashboard.Application.K8sEventPipeline.Services.Watchers;
using TrivyOperator.Dashboard.Application.K8sEventPipeline.Services.Watchers.Abstractions;
using TrivyOperator.Dashboard.Application.Queries.Trivy.Services.ClusterComplianceReports;
using TrivyOperator.Dashboard.Application.Queries.Trivy.Services.ClusterComplianceReports.Abstractions;
using TrivyOperator.Dashboard.Application.Queries.Trivy.Services.ClusterInfraAssessmentReports;
using TrivyOperator.Dashboard.Application.Queries.Trivy.Services.ClusterInfraAssessmentReports.Abstractions;
using TrivyOperator.Dashboard.Application.Queries.Trivy.Services.ClusterRbacAssessmentReports;
using TrivyOperator.Dashboard.Application.Queries.Trivy.Services.ClusterRbacAssessmentReports.Abstractions;
using TrivyOperator.Dashboard.Application.Queries.Trivy.Services.ClusterSbomReports;
using TrivyOperator.Dashboard.Application.Queries.Trivy.Services.ClusterSbomReports.Abstractions;
using TrivyOperator.Dashboard.Application.Queries.Trivy.Services.ClusterVulnerabilityReports;
using TrivyOperator.Dashboard.Application.Queries.Trivy.Services.ClusterVulnerabilityReports.Abstractions;
using TrivyOperator.Dashboard.Application.Queries.Trivy.Services.ConfigAuditReports;
using TrivyOperator.Dashboard.Application.Queries.Trivy.Services.ConfigAuditReports.Abstractions;
using TrivyOperator.Dashboard.Application.Queries.Trivy.Services.ExposedSecretReports;
using TrivyOperator.Dashboard.Application.Queries.Trivy.Services.ExposedSecretReports.Abstractions;
using TrivyOperator.Dashboard.Application.Queries.Trivy.Services.InfraAssessmentReports;
using TrivyOperator.Dashboard.Application.Queries.Trivy.Services.InfraAssessmentReports.Abstractions;
using TrivyOperator.Dashboard.Application.Queries.Trivy.Services.RbacAssessmentReports;
using TrivyOperator.Dashboard.Application.Queries.Trivy.Services.RbacAssessmentReports.Abstractions;
using TrivyOperator.Dashboard.Application.Queries.Trivy.Services.SbomReports;
using TrivyOperator.Dashboard.Application.Queries.Trivy.Services.SbomReports.Abstractions;
using TrivyOperator.Dashboard.Application.Queries.Trivy.Services.VulnerabilityReports;
using TrivyOperator.Dashboard.Application.Queries.Trivy.Services.VulnerabilityReports.Abstractions;
using TrivyOperator.Dashboard.Composition.Configuration;
using TrivyOperator.Dashboard.Domain.Kubernetes.ValueObjects;
using TrivyOperator.Dashboard.Domain.Shared.Stores.Abstractions;
using TrivyOperator.Dashboard.Domain.Trivy.Entities;
using TrivyOperator.Dashboard.Domain.Trivy.Entities.Abstracts;
using TrivyOperator.Dashboard.Domain.Trivy.ValueObjects.Shared;
using TrivyOperator.Dashboard.Infrastructure.Caching.CacheEntityCodec.Codecs;
using TrivyOperator.Dashboard.Infrastructure.Caching.CacheEntityCodec.Codecs.Abstractions;
using TrivyOperator.Dashboard.Infrastructure.Caching.ConcurrentCache;
using TrivyOperator.Dashboard.Infrastructure.Caching.ConcurrentCache.Abstractions;
using TrivyOperator.Dashboard.Infrastructure.Caching.InMemory;
using TrivyOperator.Dashboard.Infrastructure.Caching.InMemory.CacheEntries;
using TrivyOperator.Dashboard.Infrastructure.Kubernetes.CacheEntryBuilders.Abstractions;
using TrivyOperator.Dashboard.Infrastructure.Kubernetes.CustomResources;
using TrivyOperator.Dashboard.Infrastructure.Kubernetes.Mappers.Abstract;
using TrivyOperator.Dashboard.Infrastructure.Kubernetes.PersistenceAggregators;
using TrivyOperator.Dashboard.Infrastructure.Kubernetes.PersistenceAggregators.Abstracts;
using TrivyOperator.Dashboard.Infrastructure.Kubernetes.Providers;
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
        services.AddSingleton<ICrdFactory, TrivyReportCrdFactory>();
        
        services.AddSingleton<ICacheEntityCodec, BrotliMemoryPackCacheEntityCodec>();

        services.AddTrivyReportServices<ClusterComplianceReportCr, ClusterComplianceReport, Uid>(configuration);
        
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
        where TReport : class, ITrivyReport<TId>
        where TId : notnull
    {
        TrivyReportCompositionMode state =
            TrivyReportCompositionResolver.Resolve<TReport>(configuration);

        switch (state)
        {
            case TrivyReportCompositionMode.DefaultContext:
                services.AddTrivyReportDefaultContext<TReportCr, TReport, TId>();
                break;
            
            case TrivyReportCompositionMode.MultiContext:
                services.AddTrivyReportMultiContext<TReportCr, TReport, TId>();
                break;
            
            case TrivyReportCompositionMode.FileRepository:
                services.AddTrivyReportFileRepo<TReportCr, TReport, TId>();
                break;

            case TrivyReportCompositionMode.Disabled:
                services.AddTrivyQueryRelatedNullServices(typeof(TReport));
                break;


            default:
                throw new ArgumentOutOfRangeException();
        }
    }
    
    // 2nd level - add trivy reports based on running state
    
    private static void AddTrivyReportDefaultContext<TReportCr, TReport, TId>(this IServiceCollection services)
        where TReportCr : CustomResource, new()
        where TReport : ITrivyReport<TId>
        where TId : notnull
    {
        // mapper service
        services.AddReportMapper(typeof(TReport));

        // in memory cache
        // -- codec - registered above
        // services.AddSingleton<ICacheEntityCodec, BrotliMemoryPackCacheEntityCodec>();
        // -- cache entry builder
        services.AddCacheEntryBuilder(typeof(TReport));
        // services.AddSingleton<
        //     ICacheEntryBuilder<VulnerabilityReport, Digest>,
        //     VulnerabilityReportCacheEntryBuilder<VulnerabilityReport, Digest>>();
        // -- concurrent cache
        services.AddSingleton<
            IResourceConcurrentDictionaryCache<TId, CacheEntry<TReport, TId>>,
            ResourceConcurrentDictionaryCache<TId, CacheEntry<TReport, TId>>>();     
        // -- IResourceStore (in part) and IResourceProvider (out part)
        services.AddReportInMemoryCache(typeof(TReport));

        // k8s infra service, event pipeline starter, watcher
        services.AddReportKubernetesPipelineServices<TReportCr, TReport, TId>();
        
        // background queue
        services.AddSingleton<
            IKubernetesBackgroundQueue<TReportCr>,
            KubernetesBackgroundQueue<TReportCr>>();
        
        // k8s event dispatcher
        services.AddSingleton<
            IKubernetesEventDispatcher<TReportCr>,
            KubernetesEventDispatcher<TReportCr, IKubernetesBackgroundQueue<TReportCr>>>();
        
        // k8s event processor
        services.AddSingleton<
            IKubernetesEventProcessor<TReportCr>, 
            ResourceStoreUpdater<TReportCr,TReport,TId>>();
        
        // query service
        services.AddTrivyQueryRelatedServices(typeof(TReport));
    }
    
    private static void AddTrivyReportMultiContext<TReportCr, TReport, TId>(this IServiceCollection services)
        where TReportCr : CustomResource, new()
        where TReport : class, ITrivyReport<TId>
        where TId : notnull
    {
        // mapper service
        services.AddReportMapper(typeof(TReport));
        
        // aggregators
        services.AddAggregatorServices(typeof(TReport));
        
        // k8s infra services
        services.AddReportKubernetesInfraServices<TReportCr, TReport, TId>();
        
        services.AddCacheEntryBuilder(typeof(TReport));
        // services.AddSingleton<
        //     ICacheEntryBuilder<VulnerabilityReport, Digest>,
        //     VulnerabilityReportCacheEntryBuilder<VulnerabilityReport, Digest>>();
        
        // expiring cache
        services.AddSingleton<
            IExpiringResourceConcurrentDictionaryCache<TId, CacheEntry<TReport, TId>>,
            ExpiringResourceConcurrentDictionaryCache<TId, CacheEntry<TReport, TId>>>();
        
        // provider
        services.AddSingleton<
            IResourceProvider<TReport, TId>,
            KubernetesResourceProvider<TReportCr, TReport, TId>>();
        
        // query service
        services.AddTrivyQueryRelatedServices(typeof(TReport));
    }
    
    private static void AddTrivyReportFileRepo<TReportCr, TReport, TId>(this IServiceCollection services)
        where TReportCr : CustomResource, new()
        where TReport : class, ITrivyReport<TId>
        where TId : notnull
    {
        // TODO: add relevant registrations here
    }
    
    private static void AddTrivyQueryRelatedNullServices(this IServiceCollection services, Type reportType)
    {
        switch (reportType.Name)
        {
            case nameof(ClusterComplianceReport):
                services.AddScoped<IClusterComplianceReportService, ClusterComplianceReportNullService>();
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
        where TReport : ITrivyReport<TId>
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
                
                // k8s event pipeline starter
                services.AddSingleton<IKubernetesEventPipelineStarter, ClusterScopedEventPipelineStarter<TReportCr>>();
        
                // watcher
                services.AddSingleton<IClusterScopedWatcher, ClusterScopedWatcher<CustomResourceList<TReportCr>, TReportCr>>();
                
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
                
                // k8s event pipeline starter
                services.AddSingleton<IKubernetesEventPipelineStarter, NamespacedEventPipelineStarter<TReportCr>>();
            
                // watcher
                services.AddSingleton<INamespacedWatcher, NamespacedWatcher<CustomResourceList<TReportCr>, TReportCr>>();
                
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
        where TReport : ITrivyReport<TId>
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
        where TReport : class, ITrivyReport<TId>
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
