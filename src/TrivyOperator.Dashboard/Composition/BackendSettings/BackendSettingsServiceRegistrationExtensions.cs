using TrivyOperator.Dashboard.Application.History.VulnerabilityReportsHistory.Retention;
using TrivyOperator.Dashboard.Application.K8sEventPipeline.Services.Options;
using TrivyOperator.Dashboard.Application.Queries.BackendSettings.Options;
using TrivyOperator.Dashboard.Application.Queries.BackendSettings.Services;
using TrivyOperator.Dashboard.Application.Queries.BackendSettings.Services.Abstractions;
using TrivyOperator.Dashboard.Domain.History.VulnerabilityReportsHistory;
using TrivyOperator.Dashboard.Infrastructure.FileRepository.Options;

namespace TrivyOperator.Dashboard.Composition.BackendSettings;

public static class BackendSettingsServiceRegistrationExtensions
{
    public static void AddBackendSettingsRelatedServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<KubernetesOptions>(configuration.GetSection("Kubernetes"));
        services.Configure<EnabledTrivyReportsOptions>(configuration.GetSection("EnabledTrivyReports"));
        services.Configure<FileRepositoryOptions>(configuration.GetSection("FileRepository"));
        services.Configure<VulnerabilityReportsHistoryOptions>(configuration.GetSection("History"));
        services.Configure<RetentionOptions>(configuration.GetSection("History").GetSection("Retention"));
        
        services.AddScoped<IBackendSettingsService, BackendSettingsService>();
    }
}
