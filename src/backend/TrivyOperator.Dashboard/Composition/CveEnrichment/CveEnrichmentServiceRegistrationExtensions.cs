using TrivyOperator.Dashboard.Application.CveEnrichment.Abstractions;
using TrivyOperator.Dashboard.Application.CveEnrichment.Options;
using TrivyOperator.Dashboard.Application.CveEnrichment.Queries.Services;
using TrivyOperator.Dashboard.Application.CveEnrichment.Queries.Services.Abstractions;
using TrivyOperator.Dashboard.Application.CveEnrichment.Services;
using TrivyOperator.Dashboard.Application.Shared.Cache.Abstractions;
using TrivyOperator.Dashboard.Composition.Configuration;
using TrivyOperator.Dashboard.Composition.Shared;
using TrivyOperator.Dashboard.Domain.CveEnrichment.Entities;
using TrivyOperator.Dashboard.Domain.Shared.ValueObjects;
using TrivyOperator.Dashboard.Infrastructure.Caching.ConcurrentCache;
using TrivyOperator.Dashboard.Infrastructure.CveEnrichment.Services;

namespace TrivyOperator.Dashboard.Composition.CveEnrichment;

public static class CveEnrichmentServiceRegistrationExtensions
{
    // 1st level - main entrance
    public static void AddCveEnrichmentRelatedServices(this IServiceCollection services, IConfiguration configuration)
    {
        CveEnrichmentCompositionMode mode = CveEnrichmentCompositionResolver.Resolve(configuration);

        services.AddOptions<CveEnrichmentOptions>().Bind(configuration.GetSection("CveEnrichment"));

        switch (mode)
        {
            case CveEnrichmentCompositionMode.Disabled:
                CompositionLogger.Logger?.LogInformation("CVE Enrichment related services are disabled");
                services.AddScoped<ICveEnrichmentService, CveEnrichmentNullService>();
                return;

            case CveEnrichmentCompositionMode.NoVulnerabilityReports:
                CompositionLogger.Logger?.LogInformation(
                    "CVE Enrichment related services are disabled - no vulnerability reports are enabled");
                services.AddScoped<ICveEnrichmentService, CveEnrichmentNullService>();
                return;

            case CveEnrichmentCompositionMode.Enabled:
                CompositionLogger.Logger?.LogInformation("Adding CVE Enrichment related services");
                services.AddCveEnrichmentServices(configuration);
                break;

            default:
                throw new ArgumentOutOfRangeException(nameof(mode), mode, null);
        }
    }

    // 2nd level - services registration
    private static void AddCveEnrichmentServices(this IServiceCollection services, IConfiguration configuration)
    {
        // the subjects (ICache<CveId, Timestamp>) are registered by CVE Subjects
        // public - the enrichment, each source writes its own part
        services.AddSingleton<ICache<CveId, CveInfo>, Cache<CveId, CveInfo>>();

        services.AddScoped<ICveEnrichmentService, CveEnrichmentService>();

        services.AddSingleton<ICveEnrichmentRefreshTrigger, CveEnrichmentRefreshTrigger>();

        services.AddHostedService<CveEnrichmentHousekeepingHostedService>();

        services.AddCveEnrichmentSources(configuration);
    }

    // 3rd level - one hosted service per enabled source; a source is enabled or disabled at startup
    private static void AddCveEnrichmentSources(this IServiceCollection services, IConfiguration configuration)
    {
        if (configuration.LoadUseCveEnrichmentSource("Kev"))
        {
            services.AddHttpClient<IKevProvider, KevClient>(ConfigureClient);
            services.AddHostedService<KevTimedHostedService>();
        }
        else
        {
            CompositionLogger.Logger?.LogInformation("CVE Enrichment source KEV is disabled");
        }

        if (configuration.LoadUseCveEnrichmentSource("Epss"))
        {
            services.AddHttpClient<IEpssProvider, EpssClient>(ConfigureClient);
            services.AddHostedService<EpssTimedHostedService>();
        }
        else
        {
            CompositionLogger.Logger?.LogInformation("CVE Enrichment source EPSS is disabled");
        }

        if (configuration.LoadUseCveEnrichmentSource("Euvd"))
        {
            services.AddHttpClient<IEuvdProvider, EuvdClient>(ConfigureClient);
            services.AddHostedService<EuvdTimedHostedService>();
        }
        else
        {
            CompositionLogger.Logger?.LogInformation("CVE Enrichment source EUVD is disabled");
        }
    }

    private static void ConfigureClient(HttpClient client) =>
        client.DefaultRequestHeaders.UserAgent.ParseAdd(Constants.UserAgentName);
}
