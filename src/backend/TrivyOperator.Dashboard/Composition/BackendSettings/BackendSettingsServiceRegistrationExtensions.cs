using TrivyOperator.Dashboard.Application.BackendSettings.Queries.Options;
using TrivyOperator.Dashboard.Application.BackendSettings.Queries.Services;
using TrivyOperator.Dashboard.Application.BackendSettings.Queries.Services.Abstractions;
using TrivyOperator.Dashboard.Application.FileRepository.Options;
using TrivyOperator.Dashboard.Application.History.VulnerabilityReportsHistory.Retention;
using TrivyOperator.Dashboard.Application.Kubernetes.Options;
using TrivyOperator.Dashboard.Composition.Shared;
using TrivyOperator.Dashboard.Domain.History.VulnerabilityReportsHistory;
using TrivyOperator.Dashboard.Domain.History.VulnerabilityReportsHistory.Options;
using TrivyOperator.Dashboard.Infrastructure.Caching.ConcurrentCache;

namespace TrivyOperator.Dashboard.Composition.BackendSettings;

public static class BackendSettingsServiceRegistrationExtensions
{
    public static void AddBackendSettingsRelatedServices(this IServiceCollection services, IConfiguration configuration)
    {
        CompositionLogger.Logger?.LogInformation("Adding Backend Settings related services");
        
        services.Configure<KubernetesOptions>(configuration.GetSection("Kubernetes"));
        services.Configure<EnabledTrivyReportsOptions>(configuration.GetSection("EnabledTrivyReports"));
        services.Configure<FileRepositoryOptions>(configuration.GetSection("FileRepository"));
        services.Configure<VulnerabilityReportsHistoryOptions>(configuration.GetSection("History"));
        services.Configure<RetentionOptions>(configuration.GetSection("History").GetSection("Retention"));
        // all the expiring caches (resource caches, NVD) - unbound until now, so the class default was always used
        services.Configure<InMemoryCacheOptions>(configuration.GetSection("InMemoryCache"));

        services.AddScoped<IBackendSettingsService, BackendSettingsService>();
    }
}
