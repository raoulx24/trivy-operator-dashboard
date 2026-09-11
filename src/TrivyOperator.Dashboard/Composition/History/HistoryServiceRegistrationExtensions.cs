using TrivyOperator.Dashboard.Application.History.VulnerabilityReportsHistory.Retention;
using TrivyOperator.Dashboard.Application.History.VulnerabilityReportsHistory.Services;
using TrivyOperator.Dashboard.Application.K8sEventPipeline.Services.EventProcessors.Abstractions;
using TrivyOperator.Dashboard.Application.Queries.History.Services;
using TrivyOperator.Dashboard.Application.Queries.History.Services.Abstractions;
using TrivyOperator.Dashboard.Domain.History.VulnerabilityReportsHistory;
using TrivyOperator.Dashboard.Domain.History.VulnerabilityReportsHistory.Services;
using TrivyOperator.Dashboard.Domain.History.VulnerabilityReportsHistory.Services.Abstractions;
using TrivyOperator.Dashboard.Domain.History.VulnerabilityReportsHistory.Stores.Abstractions;
using TrivyOperator.Dashboard.Infrastructure.Caching.Distributed;
using TrivyOperator.Dashboard.Infrastructure.Caching.Distributed.Abstractions;
using TrivyOperator.Dashboard.Infrastructure.History.Stores;
using TrivyOperator.Dashboard.Infrastructure.Trivy.Schema.VulnerabilityReports.Models;

namespace TrivyOperator.Dashboard.Composition.History;

public static class HistoryServiceRegistrationExtensions
{
    // 1st level - main entrance
    
    public static void AddHistoryRelatedServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<VulnerabilityReportsHistoryOptions>(
            configuration.GetSection("History"));

        services.Configure<RetentionOptions>(
            configuration.GetSection("History").GetSection("Retention"));

        HistoryCompositionMode mode =
            HistoryCompositionResolver.Resolve(configuration);

        switch (mode)
        {
            case HistoryCompositionMode.Disabled:
                services.AddTransient<
                    IVulnerabilityReportsHistoryService,
                    VulnerabilityReportsHistoryNullService>();

                services.AddScoped<
                    IVulnerabilityReportsHistoryStore,
                    DistributedCacheVulnerabilityReportsHistoryNullStore>();

                break;

            case HistoryCompositionMode.DistributedCache:
                services.AddDistributedCacheHistoryServices(configuration);
                break;

            default:
                throw new ArgumentOutOfRangeException(
                    nameof(mode),
                    mode,
                    null);
        }
    }
    
    // 2nd level - services registration

    private static void AddDistributedCacheHistoryServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<DistributedCacheClientOptions>(
            configuration
                .GetSection("History")
                .GetSection("DistributedCache"));

        services.Configure<DistributedCacheClientOptions>(
            configuration
                .GetSection("History")
                .GetSection("DistributedCache")
                .GetSection("RetryOptions"));

        services.AddSingleton<DistributedCacheConnectionProvider>();
        services.AddHostedService<DistributedCacheConnectionProvider>();

        services.AddSingleton<IDistributedCacheClientFactory, DistributedCacheClientFactory>();

        services.AddSingleton<IDistributedCacheExecutor, DistributedCacheExecutor>();

        services.AddScoped<IVulnerabilityReportsHistoryStore, DistributedCacheVulnerabilityReportsHistoryStore>();

        services.AddScoped<IVulnerabilityReportsHistoryRetentionService, VulnerabilityReportsHistoryRetentionService>();

        services.AddSingleton<IKubernetesEventProcessor<VulnerabilityReportCr>, VulnerabilityReportsHistoryRefresher>();

        services.AddScoped<IVulnerabilityReportsHistoryService, VulnerabilityReportsHistoryService>();

        services.AddHostedService<VulnerabilityReportsHistoryRetentionTimedHostedService>();
    }

}
