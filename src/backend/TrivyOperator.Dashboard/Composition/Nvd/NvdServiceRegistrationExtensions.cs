using TrivyOperator.Dashboard.Application.Nvd.Abstractions;
using TrivyOperator.Dashboard.Application.Nvd.Options;
using TrivyOperator.Dashboard.Application.Nvd.Queries.Services;
using TrivyOperator.Dashboard.Application.Nvd.Queries.Services.Abstractions;
using TrivyOperator.Dashboard.Application.Shared.Cache.Abstractions;
using TrivyOperator.Dashboard.Composition.Shared;
using TrivyOperator.Dashboard.Domain.Nvd.Entities;
using TrivyOperator.Dashboard.Domain.Shared.ValueObjects;
using TrivyOperator.Dashboard.Infrastructure.Caching.ConcurrentCache;
using TrivyOperator.Dashboard.Infrastructure.Nvd.Services;

namespace TrivyOperator.Dashboard.Composition.Nvd;

public static class NvdServiceRegistrationExtensions
{
    // 1st level - main entrance
    public static void AddNvdRelatedServices(this IServiceCollection services, IConfiguration configuration)
    {
        NvdCompositionMode mode = NvdCompositionResolver.Resolve(configuration);

        services.AddOptions<NvdOptions>().Bind(configuration.GetSection("Nvd"));

        switch (mode)
        {
            case NvdCompositionMode.Disabled:
                CompositionLogger.Logger?.LogInformation("NVD related services are disabled");
                services.AddSingleton<INvdService, NvdNullService>();
                return;

            case NvdCompositionMode.NoVulnerabilityReports:
                CompositionLogger.Logger?.LogInformation(
                    "NVD related services are disabled - no vulnerability reports are enabled");
                services.AddSingleton<INvdService, NvdNullService>();
                return;

            case NvdCompositionMode.Enabled:
                CompositionLogger.Logger?.LogInformation("Adding NVD related services");
                services.AddNvdServices();
                break;

            default:
                throw new ArgumentOutOfRangeException(nameof(mode), mode, null);
        }
    }

    // 2nd level - services registration
    // all singletons - the service holds the in-flight calls; the subjects (ICache<CveId, Timestamp>) come from CVE Subjects
    private static void AddNvdServices(this IServiceCollection services)
    {
        // only Ok results, until the whole cache is not read for InMemoryCache:ExpireInMinutes
        services.AddSingleton<
            ICache<CveId, CveSourceResult<NvdInfo>>,
            ExpiringCache<CveId, CveSourceResult<NvdInfo>>>();

        services.AddSingleton<NvdRateLimiter>();

        services.AddHttpClient(NvdClient.HttpClientName, client =>
        {
            client.DefaultRequestHeaders.UserAgent.ParseAdd(Constants.UserAgentName);
        });
        services.AddSingleton<INvdProvider, NvdClient>();

        services.AddSingleton<INvdService, NvdService>();
    }
}
