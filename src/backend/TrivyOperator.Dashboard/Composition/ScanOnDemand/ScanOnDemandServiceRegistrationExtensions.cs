using TrivyOperator.Dashboard.Application.ScanOnDemand.Abstractions;
using TrivyOperator.Dashboard.Application.ScanOnDemand.HostedServices;
using TrivyOperator.Dashboard.Application.ScanOnDemand.Options;
using TrivyOperator.Dashboard.Application.ScanOnDemand.Queries.Services;
using TrivyOperator.Dashboard.Application.ScanOnDemand.Queries.Services.Abstractions;
using TrivyOperator.Dashboard.Application.ScanOnDemand.Services;
using TrivyOperator.Dashboard.Composition.Configuration;
using TrivyOperator.Dashboard.Composition.DistributedCache;
using TrivyOperator.Dashboard.Composition.Shared;
using TrivyOperator.Dashboard.Domain.ScanOnDemand.Stores.Abstractions;
using TrivyOperator.Dashboard.Infrastructure.ScanOnDemand.Kubernetes;
using TrivyOperator.Dashboard.Infrastructure.ScanOnDemand.Stores;

namespace TrivyOperator.Dashboard.Composition.ScanOnDemand;

public static class ScanOnDemandServiceRegistrationExtensions
{
    private const string NotComposableReason =
        "Scan on demand works only in the default context and without the file repository.";

    public static void AddScanOnDemandRelatedServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<ScanOnDemandOptions>(configuration.GetSection("ScanOnDemand"));

        if (!ScanOnDemandCompositionResolver.IsEnabled(configuration))
        {
            string? reason = configuration.LoadUseScanOnDemand() ? NotComposableReason : null;

            if (reason is not null)
            {
                CompositionLogger.Logger?.LogWarning("Scan on demand is enabled in the settings, but disabled: {reason}", reason);
            }
            else
            {
                CompositionLogger.Logger?.LogInformation("Scan on demand is disabled");
            }

            services.AddSingleton<IScanOnDemandQueryService>(new ScanOnDemandQueryNullService(reason));
            return;
        }

        CompositionLogger.Logger?.LogInformation("Adding Scan on demand related services");

        services.AddDistributedCacheServices(configuration);

        services.AddSingleton<IScanOnDemandAvailability, ScanOnDemandAvailability>();
        services.AddSingleton<IOnDemandScanStore, DistributedCacheOnDemandScanStore>();
        services.AddSingleton<ScanPodTemplateProvider>();
        services.AddSingleton<IScanPodClient, ScanPodClient>();
        services.AddSingleton<IScanReportReader, ScanReportReader>();
        services.AddSingleton<IScanRequestValidator, ScanRequestValidator>();
        services.AddSingleton<IOnDemandScanWakeUp, OnDemandScanWakeUp>();
        services.AddSingleton<IOnDemandScanService, OnDemandScanService>();
        services.AddSingleton<IScanOnDemandQueryService, ScanOnDemandQueryService>();
        services.AddHostedService<OnDemandScanRunnerHostedService>();
    }
}
