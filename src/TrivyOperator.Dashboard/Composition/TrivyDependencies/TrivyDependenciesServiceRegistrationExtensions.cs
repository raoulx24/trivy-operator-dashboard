using TrivyOperator.Dashboard.Application.Queries.TrivyDependencies.Services;
using TrivyOperator.Dashboard.Application.Queries.TrivyDependencies.Services.Abstractions;
using TrivyOperator.Dashboard.Composition.Configuration;
using TrivyOperator.Dashboard.Composition.Shared;
using TrivyOperator.Dashboard.Domain.Kubernetes.ValueObjects;
using TrivyOperator.Dashboard.Domain.Shared.Stores;
using TrivyOperator.Dashboard.Domain.Shared.Stores.Abstractions;
using TrivyOperator.Dashboard.Domain.Trivy.Entities;
using TrivyOperator.Dashboard.Domain.Trivy.ValueObjects.Shared;

namespace TrivyOperator.Dashboard.Composition.TrivyDependencies;

public static class TrivyDependenciesServiceRegistrationExtensions
{
    public static void AddTrivyDependenciesRelatedServices(this IServiceCollection services, IConfiguration configuration)
    {
        CompositionLogger.Logger?.LogInformation("Adding Trivy Dependencies related services");
        
        Dictionary<string, bool> enabledReports =
            configuration.LoadEnabledTrivyReports();

        services.AddNullProvidersForDisabledReports(enabledReports);

        services.AddScoped<ITrivyReportDependenciesService, TrivyReportDependenciesService>();
    }

    private static void AddNullProvidersForDisabledReports(this IServiceCollection services, IReadOnlyDictionary<string, bool> enabledReports)
    {
        if (!enabledReports.GetValueOrDefault(nameof(ConfigAuditReport)))
        {
            services.AddSingleton<IResourceProvider<ConfigAuditReport, Uid>, NullResourceRepository<ConfigAuditReport, Uid>>();
        }

        if (!enabledReports.GetValueOrDefault(nameof(ExposedSecretReport)))
        {
            services.AddSingleton<IResourceProvider<ExposedSecretReport, Digest>, NullResourceRepository<ExposedSecretReport, Digest>>();
        }

        if (!enabledReports.GetValueOrDefault(nameof(SbomReport)))
        {
            services.AddSingleton<IResourceProvider<SbomReport, Digest>, NullResourceRepository<SbomReport, Digest>>();
        }

        if (!enabledReports.GetValueOrDefault(nameof(VulnerabilityReport)))
        {
            services.AddSingleton<IResourceProvider<VulnerabilityReport, Digest>, NullResourceRepository<VulnerabilityReport, Digest>>();
        }
    }
}
