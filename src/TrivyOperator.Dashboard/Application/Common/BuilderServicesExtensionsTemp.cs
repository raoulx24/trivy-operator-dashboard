using TrivyOperator.Dashboard.Application.K8s.Pipeline;
using TrivyOperator.Dashboard.Application.K8s.Services.ResourceStoreUpdaters;
using TrivyOperator.Dashboard.Domain.K8s.Abstractions;
using TrivyOperator.Dashboard.Domain.Trivy.Entities;
using TrivyOperator.Dashboard.Domain.Trivy.ValueObjects.Shared.Identities;
using TrivyOperator.Dashboard.Infrastructure.Caching.InMemory;
using TrivyOperator.Dashboard.Infrastructure.Caching.InMemory.Abstractions;
using TrivyOperator.Dashboard.Infrastructure.Caching.InMemory.ConcurrentCache;
using TrivyOperator.Dashboard.Infrastructure.ResourceStore.Abstractions;
using TrivyOperator.Dashboard.Infrastructure.Trivy.Mappers;
using TrivyOperator.Dashboard.Infrastructure.Trivy.Mappers.Abstract;
using TrivyOperator.Dashboard.Infrastructure.Trivy.Schema.VulnerabilityReports.Models;

namespace TrivyOperator.Dashboard.Application.Common;

public static class BuilderServicesExtensionsTemp
{
    public static void AddVulnerabilityReports(this IServiceCollection services, IConfiguration configuration)
    {
        services
            .AddSingleton<IKubernetesEventProcessor<VulnerabilityReportCr>, 
                TrivyResourceStoreUpdater<VulnerabilityReportCr,VulnerabilityReport,NamespacedDigest>>();
        
        services.AddSingleton<VulnerabilityReportMapper>();

        services.AddSingleton<ITrivyReportMapper<VulnerabilityReportCr, VulnerabilityReport>>(sp =>
            sp.GetRequiredService<VulnerabilityReportMapper>());

        services.AddSingleton<ITrivyReportKeyProvider<VulnerabilityReportCr, NamespacedDigest>>(sp =>
            sp.GetRequiredService<VulnerabilityReportMapper>());

        services
            .AddSingleton<IResourceConcurrentDictionaryCache<NamespacedDigest, VulnerabilityReport>,
                ResourceConcurrentDictionaryCache<NamespacedDigest, VulnerabilityReport>>();     
        
        services.AddSingleton<InMemoryCache<VulnerabilityReport, NamespacedDigest>>();
        services.AddSingleton<IResourceStore<VulnerabilityReport, NamespacedDigest>>(sp =>
            sp.GetRequiredService<InMemoryCache<VulnerabilityReport, NamespacedDigest>>());
        services.AddSingleton<IResourceProvider<VulnerabilityReport>>(sp =>
            sp.GetRequiredService<InMemoryCache<VulnerabilityReport, NamespacedDigest>>());
    }
}
