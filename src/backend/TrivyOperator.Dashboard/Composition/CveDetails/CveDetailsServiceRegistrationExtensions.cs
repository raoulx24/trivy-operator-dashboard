using TrivyOperator.Dashboard.Application.CveDetails.Abstractions;
using TrivyOperator.Dashboard.Application.CveDetails.Options;
using TrivyOperator.Dashboard.Application.CveDetails.Queries.Services;
using TrivyOperator.Dashboard.Application.CveDetails.Queries.Services.Abstractions;
using TrivyOperator.Dashboard.Application.CveDetails.Services;
using TrivyOperator.Dashboard.Application.Shared.Cache.Abstractions;
using TrivyOperator.Dashboard.Application.Shared.CveSubjects.Abstractions;
using TrivyOperator.Dashboard.Composition.Configuration;
using TrivyOperator.Dashboard.Composition.Shared;
using TrivyOperator.Dashboard.Domain.CveDetails.Entities;
using TrivyOperator.Dashboard.Domain.Kubernetes.ValueObjects;
using TrivyOperator.Dashboard.Domain.Shared.Stores.Abstractions;
using TrivyOperator.Dashboard.Domain.Shared.ValueObjects;
using TrivyOperator.Dashboard.Domain.Trivy.Entities;
using TrivyOperator.Dashboard.Domain.Trivy.ValueObjects.Shared;
using TrivyOperator.Dashboard.Infrastructure.Caching.ConcurrentCache;
using TrivyOperator.Dashboard.Infrastructure.CveOrg.Services;
using TrivyOperator.Dashboard.Infrastructure.Nvd.Services;

namespace TrivyOperator.Dashboard.Composition.CveDetails;

public static class CveDetailsServiceRegistrationExtensions
{
    // 1st level - main entrance
    public static void AddCveDetailsRelatedServices(this IServiceCollection services, IConfiguration configuration)
    {
        CveDetailsCompositionMode mode = CveDetailsCompositionResolver.Resolve(configuration);

        services.AddOptions<CveDetailsOptions>().Bind(configuration.GetSection("CveDetails"));

        switch (mode)
        {
            case CveDetailsCompositionMode.Disabled:
                CompositionLogger.Logger?.LogInformation("CVE Details related services are disabled");
                services.AddSingleton<ICveDetailsService, CveDetailsNullService>();
                return;

            case CveDetailsCompositionMode.NoVulnerabilityReports:
                CompositionLogger.Logger?.LogInformation(
                    "CVE Details related services are disabled - no vulnerability reports are enabled");
                services.AddSingleton<ICveDetailsService, CveDetailsNullService>();
                return;

            case CveDetailsCompositionMode.Enabled:
                CompositionLogger.Logger?.LogInformation("Adding CVE Details related services");
                services.AddCveDetailsServices(
                    configuration,
                    sp => new CveSubjectsCveDetailsSubjects(sp.GetRequiredService<ICveSubjects>()));
                break;

            case CveDetailsCompositionMode.EnabledOnDemand:
                CompositionLogger.Logger?.LogInformation(
                    "Adding CVE Details related services - the CVEs are looked for in the vulnerability reports of the current context");
                services.AddCveDetailsServices(
                    configuration,
                    sp => new ReportEntriesCveDetailsSubjects(
                        sp.GetService<IResourceEntryProvider<VulnerabilityReportSlim, Digest>>(),
                        sp.GetService<IResourceEntryProvider<ClusterVulnerabilityReportSlim, Uid>>()));
                break;

            case CveDetailsCompositionMode.AnyCve:
                CompositionLogger.Logger?.LogWarning(
                    "Adding CVE Details related services - any CVE can be looked up (CveDetails:AllowAnyCve), the dashboard is a proxy to the provider");
                services.AddCveDetailsServices(configuration, subjectsFactory: null);
                break;

            default:
                throw new ArgumentOutOfRangeException(nameof(mode), mode, null);
        }
    }

    // 2nd level - services registration
    // all singletons - the service holds the in-flight calls
    // not gated (CveDetails:AllowAnyCve, no subjects factory) - the service gets no subjects and looks up any CVE
    private static void AddCveDetailsServices(
        this IServiceCollection services,
        IConfiguration configuration,
        Func<IServiceProvider, ICveDetailsSubjects>? subjectsFactory)
    {
        // only Ok results, until the whole cache is not read for InMemoryCache:ExpireInMinutes
        services.AddSingleton<
            ICache<CveId, CveSourceResult<CveDetailsInfo>>,
            ExpiringCache<CveId, CveSourceResult<CveDetailsInfo>>>();

        services.AddCveDetailsProvider(configuration);

        services.AddSingleton<ICveDetailsService>(sp =>
            new CveDetailsService(
                subjectsFactory?.Invoke(sp),
                sp.GetRequiredService<ICache<CveId, CveSourceResult<CveDetailsInfo>>>(),
                sp.GetRequiredService<ICveDetailsProvider>()));
    }

    // 3rd level - the provider chosen by CveDetails:Provider; an unknown one stops the startup, not a silent fallback
    private static void AddCveDetailsProvider(this IServiceCollection services, IConfiguration configuration)
    {
        string provider = configuration.LoadCveDetailsProvider();

        if (string.Equals(provider, CveDetailsProviders.CveOrg, StringComparison.OrdinalIgnoreCase))
        {
            CompositionLogger.Logger?.LogInformation("CVE Details provider: CVE.org");

            services.AddOptions<CveOrgOptions>().Bind(configuration.GetSection("CveDetails:CveOrg"));
            services.AddHttpClient(CveOrgClient.HttpClientName, client =>
            {
                client.DefaultRequestHeaders.UserAgent.ParseAdd(Constants.UserAgentName);
            });
            services.AddSingleton<ICveDetailsProvider, CveOrgClient>();

            return;
        }

        if (string.Equals(provider, CveDetailsProviders.Nvd, StringComparison.OrdinalIgnoreCase))
        {
            CompositionLogger.Logger?.LogInformation("CVE Details provider: NVD");

            services.AddOptions<NvdOptions>().Bind(configuration.GetSection("CveDetails:Nvd"));
            services.AddSingleton<NvdRateLimiter>();
            services.AddHttpClient(NvdClient.HttpClientName, client =>
            {
                client.DefaultRequestHeaders.UserAgent.ParseAdd(Constants.UserAgentName);
            });
            services.AddSingleton<ICveDetailsProvider, NvdClient>();

            return;
        }

        throw new InvalidOperationException(
            $"CveDetails:Provider '{provider}' is not known - expected {CveDetailsProviders.CveOrg} or {CveDetailsProviders.Nvd}");
    }
}
