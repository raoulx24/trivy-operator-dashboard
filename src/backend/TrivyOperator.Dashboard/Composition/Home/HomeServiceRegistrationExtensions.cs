using TrivyOperator.Dashboard.Application.Home.Queries.Services;
using TrivyOperator.Dashboard.Application.Home.Queries.Services.Abstractions;
using TrivyOperator.Dashboard.Application.Home.Services;
using TrivyOperator.Dashboard.Composition.Configuration;
using TrivyOperator.Dashboard.Composition.Shared;
using TrivyOperator.Dashboard.Composition.Trivy;
using TrivyOperator.Dashboard.Domain.Kubernetes.ValueObjects;
using TrivyOperator.Dashboard.Domain.Shared.Stores.Abstractions;
using TrivyOperator.Dashboard.Domain.Trivy.Entities;
using TrivyOperator.Dashboard.Domain.Trivy.ValueObjects.Shared;
using TrivyOperator.Dashboard.Domain.Trivy.ValueObjects.Vulnerabilities;
using TrivyOperator.Dashboard.Infrastructure.Caching.InMemory.Details;

namespace TrivyOperator.Dashboard.Composition.Home;

public static class HomeServiceRegistrationExtensions
{
    // 1st level - main entrance
    // the home page reads the slim reports and the details stores, which exist only for the default context
    // (multi-context and file repository will have a home page of their own)
    public static void AddHomeRelatedServices(this IServiceCollection services, IConfiguration configuration)
    {
        if (configuration.LoadUseFileRepository() || !configuration.LoadUseDefaultContext())
        {
            CompositionLogger.Logger?.LogInformation("Home related services are disabled - not in the default context");
            services.AddScoped<IHomeService, HomeNullService>();
            return;
        }

        CompositionLogger.Logger?.LogInformation("Adding Home related services");
        services.AddHomeServices();
    }

    // 2nd level - services registration
    private static void AddHomeServices(this IServiceCollection services)
    {
        // the providers of the report types that are not enabled are not registered (null)
        services.AddSingleton(sp => new HomeReportSources
        {
            VulnerabilityReports = sp.GetService<IResourceProvider<VulnerabilityReportSlim, Digest>>(),
            ClusterVulnerabilityReports = sp.GetService<IResourceProvider<ClusterVulnerabilityReportSlim, Uid>>(),
            VulnerabilityDefinitions = sp.GetKeyedService<InMemoryDetailRepository<VulnerabilityDefinition, VulnerabilityId>>(
                DetailFamilies.Vulnerability),
            PackageVulnerabilities = sp.GetKeyedService<InMemoryDetailRepository<PackageVulnerability, PackageVulnerabilityKey>>(
                DetailFamilies.Vulnerability),
            ConfigAuditReports = sp.GetService<IResourceProvider<ConfigAuditReport, Uid>>(),
            ClusterConfigAuditReports = sp.GetService<IResourceProvider<ClusterConfigAuditReport, Uid>>(),
            RbacAssessmentReports = sp.GetService<IResourceProvider<RbacAssessmentReport, Uid>>(),
            ClusterRbacAssessmentReports = sp.GetService<IResourceProvider<ClusterRbacAssessmentReport, Uid>>(),
            InfraAssessmentReports = sp.GetService<IResourceProvider<InfraAssessmentReport, Uid>>(),
            ClusterInfraAssessmentReports = sp.GetService<IResourceProvider<ClusterInfraAssessmentReport, Uid>>(),
        });

        services.AddSingleton<ActiveVulnerabilityTableCache>();
        services.AddScoped<ActiveVulnerabilityTableBuilder>();

        services.AddScoped<IHomeService, HomeService>();
    }
}
