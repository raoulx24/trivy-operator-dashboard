using TrivyOperator.Dashboard.Application.K8s.Pipeline;
using TrivyOperator.Dashboard.Application.K8s.Services.BackgroundQueues;
using TrivyOperator.Dashboard.Application.K8s.Services.EventCoordinators;
using TrivyOperator.Dashboard.Application.K8s.Services.EventCoordinators.Abstractions;
using TrivyOperator.Dashboard.Application.K8s.Services.EventDispatchers;
using TrivyOperator.Dashboard.Application.K8s.Services.ResourceStoreUpdaters;
using TrivyOperator.Dashboard.Application.K8s.Services.WatcherEvents;
using TrivyOperator.Dashboard.Application.K8s.Services.Watchers;
using TrivyOperator.Dashboard.Application.K8s.Services.Watchers.Abstractions;
using TrivyOperator.Dashboard.Domain.K8s.Abstractions;
using TrivyOperator.Dashboard.Domain.K8s.ValueObjects;
using TrivyOperator.Dashboard.Domain.Trivy.Entities;
using TrivyOperator.Dashboard.Domain.Trivy.Entities.Abstracts;
using TrivyOperator.Dashboard.Domain.Trivy.ValueObjects.Shared;
using TrivyOperator.Dashboard.Infrastructure.Caching.ConcurrentCache;
using TrivyOperator.Dashboard.Infrastructure.Caching.ConcurrentCache.Abstractions;
using TrivyOperator.Dashboard.Infrastructure.Caching.InMemory;
using TrivyOperator.Dashboard.Infrastructure.Caching.InMemory.CacheEntries;
using TrivyOperator.Dashboard.Infrastructure.K8s.CustomResources;
using TrivyOperator.Dashboard.Infrastructure.K8s.Services;
using TrivyOperator.Dashboard.Infrastructure.K8s.Services.Abstractions;
using TrivyOperator.Dashboard.Infrastructure.Persistence.CacheEntityCodec;
using TrivyOperator.Dashboard.Infrastructure.Persistence.CacheEntityCodec.Abstractions;
using TrivyOperator.Dashboard.Infrastructure.Persistence.Trivy.Builders;
using TrivyOperator.Dashboard.Infrastructure.Persistence.Trivy.Builders.Abstractions;
using TrivyOperator.Dashboard.Infrastructure.Trivy.Mappers;
using TrivyOperator.Dashboard.Infrastructure.Trivy.Mappers.Abstract;
using TrivyOperator.Dashboard.Infrastructure.Trivy.Schema.ClusterComplianceReports;
using TrivyOperator.Dashboard.Infrastructure.Trivy.Schema.ConfigAuditReports;
using TrivyOperator.Dashboard.Infrastructure.Trivy.Schema.ExposedSecretReports;
using TrivyOperator.Dashboard.Infrastructure.Trivy.Schema.InfraAssessmentReports;
using TrivyOperator.Dashboard.Infrastructure.Trivy.Schema.RbacAssessmentReports;
using TrivyOperator.Dashboard.Infrastructure.Trivy.Schema.SbomReports;
using TrivyOperator.Dashboard.Infrastructure.Trivy.Schema.VulnerabilityReports.Models;

namespace TrivyOperator.Dashboard.Application.Common;

public static class BuilderServicesExtensionsTemp
{
    public static void AddTrivyReports(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddNamespacedTrivyReport<VulnerabilityReportCr, VulnerabilityReport, Digest>(configuration);
        
        
    }

    public static void AddNamespacedTrivyReport<TReportCr, TReport, TId>(this IServiceCollection services, IConfiguration configuration)
    where TReportCr : CustomResource, new()
    where TReport : ITrivyReport<TId>
    where TId : notnull
    {
        // mapper service
        services.AddReportMapper(typeof(TReport));
            // services.AddSingleton<VulnerabilityReportMapper>();
            // services.AddSingleton<ITrivyReportMapper<VulnerabilityReportCr, VulnerabilityReport>>(sp =>
            //     sp.GetRequiredService<VulnerabilityReportMapper>());
            // services.AddSingleton<ITrivyReportKeyProvider<VulnerabilityReportCr, Digest>>(sp =>
            //     sp.GetRequiredService<VulnerabilityReportMapper>());

        // in memory cache
        // -- codec
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
        services.AddSingleton<InMemoryImageReportCache<VulnerabilityReport>>();
        services.AddSingleton<IResourceStore<VulnerabilityReport, Digest>>(sp =>
            sp.GetRequiredService<InMemoryImageReportCache<VulnerabilityReport>>());
        services.AddSingleton<IResourceProvider<VulnerabilityReport>>(sp =>
            sp.GetRequiredService<InMemoryImageReportCache<VulnerabilityReport>>());
        
        services.AddNamespacedWatcherPipeline<TReportCr, TReport, TId>();
    }

    private static void AddNamespacedWatcherPipeline<TReportCr, TReport, TId>(this IServiceCollection services)
    where TReportCr : CustomResource, new()
    where TReport : ITrivyReport<TId>
    {
        // k8s infra service
        services
            .AddSingleton<
                INamespacedResourceService<TReportCr, CustomResourceList<TReportCr>>,
                NamespacedCustomResourceService<TReportCr>>();
        
        // k8s event coordinator
        services.AddSingleton<INamespacedKubernetesEventCoordinator,
            NamespacedKubernetesEventCoordinator<IKubernetesEventDispatcher<TReportCr>,
                INamespacedWatcher<TReportCr>, TReportCr>>();
        
        // watcher
        services.AddSingleton<INamespacedWatcher<TReportCr>, NamespacedWatcher<
            CustomResourceList<TReportCr>, TReportCr,
            IKubernetesBackgroundQueue<TReportCr>, WatcherEvent<TReportCr>>>();
        
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
                TrivyResourceStoreUpdater<TReportCr,TReport,TId>>();
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
            services.AddSingleton<ITrivyReportMapper<ClusterComplianceReportCr, ClusterComplianceReport>>(sp =>
                sp.GetRequiredService<ClusterComplianceReportMapper>());
            services.AddSingleton<ITrivyReportKeyProvider<ClusterComplianceReportCr, Uid>>(sp =>
                sp.GetRequiredService<ClusterComplianceReportMapper>());
            break;

        case nameof(ClusterConfigAuditReport):
            services.AddSingleton<ClusterConfigAuditReportMapper>();
            services.AddSingleton<ITrivyReportMapper<ClusterConfigAuditReportCr, ClusterConfigAuditReport>>(sp =>
                sp.GetRequiredService<ClusterConfigAuditReportMapper>());
            services.AddSingleton<ITrivyReportKeyProvider<ClusterConfigAuditReportCr, Uid>>(sp =>
                sp.GetRequiredService<ClusterConfigAuditReportMapper>());
            break;

        case nameof(ClusterInfraAssessmentReport):
            services.AddSingleton<ClusterInfraAssessmentReportMapper>();
            services.AddSingleton<ITrivyReportMapper<ClusterInfraAssessmentReportCr, ClusterInfraAssessmentReport>>(sp =>
                sp.GetRequiredService<ClusterInfraAssessmentReportMapper>());
            services.AddSingleton<ITrivyReportKeyProvider<ClusterInfraAssessmentReportCr, Uid>>(sp =>
                sp.GetRequiredService<ClusterInfraAssessmentReportMapper>());
            break;

        case nameof(ClusterRbacAssessmentReport):
            services.AddSingleton<ClusterRbacAssessmentReportMapper>();
            services.AddSingleton<ITrivyReportMapper<ClusterRbacAssessmentReportCr, ClusterRbacAssessmentReport>>(sp =>
                sp.GetRequiredService<ClusterRbacAssessmentReportMapper>());
            services.AddSingleton<ITrivyReportKeyProvider<ClusterRbacAssessmentReportCr, Uid>>(sp =>
                sp.GetRequiredService<ClusterRbacAssessmentReportMapper>());
            break;

        case nameof(ClusterSbomReport):
            services.AddSingleton<ClusterSbomReportMapper>();
            services.AddSingleton<ITrivyReportMapper<ClusterSbomReportCr, ClusterSbomReport>>(sp =>
                sp.GetRequiredService<ClusterSbomReportMapper>());
            services.AddSingleton<ITrivyReportKeyProvider<ClusterSbomReportCr, Uid>>(sp =>
                sp.GetRequiredService<ClusterSbomReportMapper>());
            break;

        case nameof(ClusterVulnerabilityReport):
            services.AddSingleton<ClusterVulnerabilityReportMapper>();
            services.AddSingleton<ITrivyReportMapper<ClusterVulnerabilityReportCr, ClusterVulnerabilityReport>>(sp =>
                sp.GetRequiredService<ClusterVulnerabilityReportMapper>());
            services.AddSingleton<ITrivyReportKeyProvider<ClusterVulnerabilityReportCr, Uid>>(sp =>
                sp.GetRequiredService<ClusterVulnerabilityReportMapper>());
            break;

        case nameof(ConfigAuditReport):
            services.AddSingleton<ConfigAuditReportMapper>();
            services.AddSingleton<ITrivyReportMapper<ConfigAuditReportCr, ConfigAuditReport>>(sp =>
                sp.GetRequiredService<ConfigAuditReportMapper>());
            services.AddSingleton<ITrivyReportKeyProvider<ConfigAuditReportCr, Uid>>(sp =>
                sp.GetRequiredService<ConfigAuditReportMapper>());
            break;

        case nameof(ExposedSecretReport):
            services.AddSingleton<ExposedSecretReportMapper>();
            services.AddSingleton<ITrivyReportMapper<ExposedSecretReportCr, ExposedSecretReport>>(sp =>
                sp.GetRequiredService<ExposedSecretReportMapper>());
            services.AddSingleton<ITrivyReportKeyProvider<ExposedSecretReportCr, Digest>>(sp =>
                sp.GetRequiredService<ExposedSecretReportMapper>());
            break;

        case nameof(InfraAssessmentReport):
            services.AddSingleton<InfraAssessmentReportMapper>();
            services.AddSingleton<ITrivyReportMapper<InfraAssessmentReportCr, InfraAssessmentReport>>(sp =>
                sp.GetRequiredService<InfraAssessmentReportMapper>());
            services.AddSingleton<ITrivyReportKeyProvider<InfraAssessmentReportCr, Uid>>(sp =>
                sp.GetRequiredService<InfraAssessmentReportMapper>());
            break;

        case nameof(RbacAssessmentReport):
            services.AddSingleton<RbacAssessmentReportMapper>();
            services.AddSingleton<ITrivyReportMapper<RbacAssessmentReportCr, RbacAssessmentReport>>(sp =>
                sp.GetRequiredService<RbacAssessmentReportMapper>());
            services.AddSingleton<ITrivyReportKeyProvider<RbacAssessmentReportCr, Uid>>(sp =>
                sp.GetRequiredService<RbacAssessmentReportMapper>());
            break;

        case nameof(SbomReport):
            services.AddSingleton<SbomReportMapper>();
            services.AddSingleton<ITrivyReportMapper<SbomReportCr, SbomReport>>(sp =>
                sp.GetRequiredService<SbomReportMapper>());
            services.AddSingleton<ITrivyReportKeyProvider<SbomReportCr, Digest>>(sp =>
                sp.GetRequiredService<SbomReportMapper>());
            break;

        case nameof(VulnerabilityReport):
            services.AddSingleton<VulnerabilityReportMapper>();
            services.AddSingleton<ITrivyReportMapper<VulnerabilityReportCr, VulnerabilityReport>>(sp =>
                sp.GetRequiredService<VulnerabilityReportMapper>());
            services.AddSingleton<ITrivyReportKeyProvider<VulnerabilityReportCr, Digest>>(sp =>
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
            services.AddSingleton<IResourceProvider<ClusterComplianceReport>>(sp =>
                sp.GetRequiredService<InMemoryResourceReportCache<ClusterComplianceReport>>());
            break;

        case nameof(ClusterConfigAuditReport):
            services.AddSingleton<InMemoryResourceReportCache<ClusterConfigAuditReport>>();
            services.AddSingleton<IResourceStore<ClusterConfigAuditReport, Uid>>(sp =>
                sp.GetRequiredService<InMemoryResourceReportCache<ClusterConfigAuditReport>>());
            services.AddSingleton<IResourceProvider<ClusterConfigAuditReport>>(sp =>
                sp.GetRequiredService<InMemoryResourceReportCache<ClusterConfigAuditReport>>());
            break;

        case nameof(ClusterInfraAssessmentReport):
            services.AddSingleton<InMemoryResourceReportCache<ClusterInfraAssessmentReport>>();
            services.AddSingleton<IResourceStore<ClusterInfraAssessmentReport, Uid>>(sp =>
                sp.GetRequiredService<InMemoryResourceReportCache<ClusterInfraAssessmentReport>>());
            services.AddSingleton<IResourceProvider<ClusterInfraAssessmentReport>>(sp =>
                sp.GetRequiredService<InMemoryResourceReportCache<ClusterInfraAssessmentReport>>());
            break;

        case nameof(ClusterRbacAssessmentReport):
            services.AddSingleton<InMemoryResourceReportCache<ClusterRbacAssessmentReport>>();
            services.AddSingleton<IResourceStore<ClusterRbacAssessmentReport, Uid>>(sp =>
                sp.GetRequiredService<InMemoryResourceReportCache<ClusterRbacAssessmentReport>>());
            services.AddSingleton<IResourceProvider<ClusterRbacAssessmentReport>>(sp =>
                sp.GetRequiredService<InMemoryResourceReportCache<ClusterRbacAssessmentReport>>());
            break;

        case nameof(ClusterSbomReport):
            services.AddSingleton<InMemoryResourceReportCache<ClusterSbomReport>>();
            services.AddSingleton<IResourceStore<ClusterSbomReport, Uid>>(sp =>
                sp.GetRequiredService<InMemoryResourceReportCache<ClusterSbomReport>>());
            services.AddSingleton<IResourceProvider<ClusterSbomReport>>(sp =>
                sp.GetRequiredService<InMemoryResourceReportCache<ClusterSbomReport>>());
            break;

        case nameof(ClusterVulnerabilityReport):
            services.AddSingleton<InMemoryResourceReportCache<ClusterVulnerabilityReport>>();
            services.AddSingleton<IResourceStore<ClusterVulnerabilityReport, Uid>>(sp =>
                sp.GetRequiredService<InMemoryResourceReportCache<ClusterVulnerabilityReport>>());
            services.AddSingleton<IResourceProvider<ClusterVulnerabilityReport>>(sp =>
                sp.GetRequiredService<InMemoryResourceReportCache<ClusterVulnerabilityReport>>());
            break;

        case nameof(ConfigAuditReport):
            services.AddSingleton<InMemoryResourceReportCache<ConfigAuditReport>>();
            services.AddSingleton<IResourceStore<ConfigAuditReport, Uid>>(sp =>
                sp.GetRequiredService<InMemoryResourceReportCache<ConfigAuditReport>>());
            services.AddSingleton<IResourceProvider<ConfigAuditReport>>(sp =>
                sp.GetRequiredService<InMemoryResourceReportCache<ConfigAuditReport>>());
            break;

        case nameof(ExposedSecretReport):
            services.AddSingleton<InMemoryImageReportCache<ExposedSecretReport>>();
            services.AddSingleton<IResourceStore<ExposedSecretReport, Digest>>(sp =>
                sp.GetRequiredService<InMemoryImageReportCache<ExposedSecretReport>>());
            services.AddSingleton<IResourceProvider<ExposedSecretReport>>(sp =>
                sp.GetRequiredService<InMemoryImageReportCache<ExposedSecretReport>>());
            break;

        case nameof(InfraAssessmentReport):
            services.AddSingleton<InMemoryResourceReportCache<InfraAssessmentReport>>();
            services.AddSingleton<IResourceStore<InfraAssessmentReport, Uid>>(sp =>
                sp.GetRequiredService<InMemoryResourceReportCache<InfraAssessmentReport>>());
            services.AddSingleton<IResourceProvider<InfraAssessmentReport>>(sp =>
                sp.GetRequiredService<InMemoryResourceReportCache<InfraAssessmentReport>>());
            break;

        case nameof(RbacAssessmentReport):
            services.AddSingleton<InMemoryResourceReportCache<RbacAssessmentReport>>();
            services.AddSingleton<IResourceStore<RbacAssessmentReport, Uid>>(sp =>
                sp.GetRequiredService<InMemoryResourceReportCache<RbacAssessmentReport>>());
            services.AddSingleton<IResourceProvider<RbacAssessmentReport>>(sp =>
                sp.GetRequiredService<InMemoryResourceReportCache<RbacAssessmentReport>>());
            break;

        case nameof(SbomReport):
            services.AddSingleton<InMemoryImageReportCache<SbomReport>>();
            services.AddSingleton<IResourceStore<SbomReport, Digest>>(sp =>
                sp.GetRequiredService<InMemoryImageReportCache<SbomReport>>());
            services.AddSingleton<IResourceProvider<SbomReport>>(sp =>
                sp.GetRequiredService<InMemoryImageReportCache<SbomReport>>());
            break;

        case nameof(VulnerabilityReport):
            services.AddSingleton<InMemoryImageReportCache<VulnerabilityReport>>();
            services.AddSingleton<IResourceStore<VulnerabilityReport, Digest>>(sp =>
                sp.GetRequiredService<InMemoryImageReportCache<VulnerabilityReport>>());
            services.AddSingleton<IResourceProvider<VulnerabilityReport>>(sp =>
                sp.GetRequiredService<InMemoryImageReportCache<VulnerabilityReport>>());
            break;

        default:
            throw new NotSupportedException(
                $"No report cache registered for report type '{reportType.Name}'.");
    }
}
}
