using TrivyOperator.Dashboard.Application.TrivyDependencies.Queries.Services;
using TrivyOperator.Dashboard.Application.TrivyDependencies.Queries.Services.Abstractions;
using TrivyOperator.Dashboard.Composition.Shared;

namespace TrivyOperator.Dashboard.Composition.TrivyDependencies;

public static class TrivyDependenciesServiceRegistrationExtensions
{
    // the providers of the disabled reports are empty ones, registered by the Trivy reports composition
    public static void AddTrivyDependenciesRelatedServices(this IServiceCollection services, IConfiguration configuration)
    {
        CompositionLogger.Logger?.LogInformation("Adding Trivy Dependencies related services");

        services.AddScoped<ITrivyReportDependenciesService, TrivyReportDependenciesService>();
    }
}
