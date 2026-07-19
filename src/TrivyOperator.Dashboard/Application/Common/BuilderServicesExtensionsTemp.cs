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
using TrivyOperator.Dashboard.Domain.Trivy.Entities;
using TrivyOperator.Dashboard.Domain.Trivy.ValueObjects.Shared.Identities;
using TrivyOperator.Dashboard.Infrastructure.Caching.InMemory;
using TrivyOperator.Dashboard.Infrastructure.Caching.InMemory.Abstractions;
using TrivyOperator.Dashboard.Infrastructure.Caching.InMemory.ConcurrentCache;
using TrivyOperator.Dashboard.Infrastructure.K8s.CustomResources;
using TrivyOperator.Dashboard.Infrastructure.K8s.Services;
using TrivyOperator.Dashboard.Infrastructure.K8s.Services.Abstractions;
using TrivyOperator.Dashboard.Infrastructure.Trivy.Mappers;
using TrivyOperator.Dashboard.Infrastructure.Trivy.Mappers.Abstract;
using TrivyOperator.Dashboard.Infrastructure.Trivy.Schema.VulnerabilityReports.Models;

namespace TrivyOperator.Dashboard.Application.Common;

public static class BuilderServicesExtensionsTemp
{
    public static void AddVulnerabilityReports(this IServiceCollection services, IConfiguration configuration)
    {
        // mapper service
        services.AddSingleton<VulnerabilityReportMapper>();
        services.AddSingleton<ITrivyReportMapper<VulnerabilityReportCr, VulnerabilityReport>>(sp =>
            sp.GetRequiredService<VulnerabilityReportMapper>());
        services.AddSingleton<ITrivyReportKeyProvider<VulnerabilityReportCr, NamespacedDigest>>(sp =>
            sp.GetRequiredService<VulnerabilityReportMapper>());

        // in memory cache
        services
            .AddSingleton<IResourceConcurrentDictionaryCache<NamespacedDigest, VulnerabilityReport>,
                ResourceConcurrentDictionaryCache<NamespacedDigest, VulnerabilityReport>>();     
        
        services.AddSingleton<InMemoryResourceCache<VulnerabilityReport, NamespacedDigest>>();
        services.AddSingleton<IResourceStore<VulnerabilityReport, NamespacedDigest>>(sp =>
            sp.GetRequiredService<InMemoryResourceCache<VulnerabilityReport, NamespacedDigest>>());
        services.AddSingleton<IResourceProvider<VulnerabilityReport>>(sp =>
            sp.GetRequiredService<InMemoryResourceCache<VulnerabilityReport, NamespacedDigest>>());
        
        // k8s infra service
        services
            .AddSingleton<
                INamespacedResourceWatchService<VulnerabilityReportCr, CustomResourceList<VulnerabilityReportCr>>,
                NamespacedCustomResourceService<VulnerabilityReportCr>>();
        
        // k8s event coordinator
        services.AddSingleton<INamespacedKubernetesEventCoordinator,
            NamespacedKubernetesEventCoordinator<IKubernetesEventDispatcher<VulnerabilityReportCr>,
                INamespacedWatcher<VulnerabilityReportCr>, VulnerabilityReportCr>>();
        
        // watcher
        services.AddSingleton<INamespacedWatcher<VulnerabilityReportCr>, NamespacedWatcher<
            CustomResourceList<VulnerabilityReportCr>, VulnerabilityReportCr,
            IKubernetesBackgroundQueue<VulnerabilityReportCr>, WatcherEvent<VulnerabilityReportCr>>>();
        
        // background queue
        services
            .AddSingleton<IKubernetesBackgroundQueue<VulnerabilityReportCr>,
                KubernetesBackgroundQueue<VulnerabilityReportCr>>();
        
        // k8s event dispatcher
        services.AddSingleton<IKubernetesEventDispatcher<VulnerabilityReportCr>,
            KubernetesEventDispatcher<VulnerabilityReportCr,
                IKubernetesBackgroundQueue<VulnerabilityReportCr>>>();
        
        // k8s event processor
        services
            .AddSingleton<IKubernetesEventProcessor<VulnerabilityReportCr>, 
                TrivyResourceStoreUpdater<VulnerabilityReportCr,VulnerabilityReport,NamespacedDigest>>();
    }
}
