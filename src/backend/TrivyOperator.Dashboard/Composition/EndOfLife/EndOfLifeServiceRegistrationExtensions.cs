using TrivyOperator.Dashboard.Application.EndOfLife.Abstractions;
using TrivyOperator.Dashboard.Application.EndOfLife.KubernetesEventProcessors;
using TrivyOperator.Dashboard.Application.EndOfLife.Models;
using TrivyOperator.Dashboard.Application.EndOfLife.Options;
using TrivyOperator.Dashboard.Application.EndOfLife.Queries.Services;
using TrivyOperator.Dashboard.Application.EndOfLife.Queries.Services.Abstractions;
using TrivyOperator.Dashboard.Application.EndOfLife.Services;
using TrivyOperator.Dashboard.Application.Kubernetes.EventPipeline.EventProcessors.Abstractions;
using TrivyOperator.Dashboard.Application.Shared.Cache.Abstractions;
using TrivyOperator.Dashboard.Composition.Configuration;
using TrivyOperator.Dashboard.Composition.Shared;
using TrivyOperator.Dashboard.Domain.EndOfLife.Entities;
using TrivyOperator.Dashboard.Domain.EndOfLife.ValueObjects;
using TrivyOperator.Dashboard.Domain.Kubernetes.ValueObjects;
using TrivyOperator.Dashboard.Domain.Shared.ValueObjects;
using TrivyOperator.Dashboard.Domain.Trivy.Entities;
using TrivyOperator.Dashboard.Domain.Trivy.ValueObjects.Shared;
using TrivyOperator.Dashboard.Infrastructure.Caching.ConcurrentCache;
using TrivyOperator.Dashboard.Infrastructure.EndOfLife.Services;

namespace TrivyOperator.Dashboard.Composition.EndOfLife;

public static class EndOfLifeServiceRegistrationExtensions
{
    // 1st level - main entrance
    public static void AddEndOfLifeRelatedServices(this IServiceCollection services, IConfiguration configuration)
    {
        EndOfLifeCompositionMode mode = EndOfLifeCompositionResolver.Resolve(configuration);

        // product mappings come from endoflife.config.json (+ optional ProductMappingsFile), see the configuration reader
        services.AddOptions<EndOfLifeOptions>().Bind(configuration.GetSection("EndOfLife"));

        switch (mode)
        {
            case EndOfLifeCompositionMode.Disabled:
                CompositionLogger.Logger?.LogInformation("End of Life related services are disabled");
                services.AddScoped<IEndOfLifeService, EndOfLifeNullService>();
                return;

            case EndOfLifeCompositionMode.NoVulnerabilityReports:
                CompositionLogger.Logger?.LogInformation(
                    "End of Life related services are disabled - no vulnerability reports are enabled");
                services.AddScoped<IEndOfLifeService, EndOfLifeNullService>();
                return;

            case EndOfLifeCompositionMode.Enabled:
                CompositionLogger.Logger?.LogInformation("Adding End of Life related services");
                services.AddEndOfLifeServices(configuration);
                break;

            default:
                throw new ArgumentOutOfRangeException(nameof(mode), mode, null);
        }
    }

    // 2nd level - services registration
    private static void AddEndOfLifeServices(this IServiceCollection services, IConfiguration configuration)
    {
        // subjects collected by the processors
        services.AddSingleton<ICache<EolSubject, Timestamp>, Cache<EolSubject, Timestamp>>();
        // internal - per product endoflife.date responses
        services.AddSingleton<ICache<ProductId, EolProductFetch>, Cache<ProductId, EolProductFetch>>();
        // public - resolved info, consumed by the service
        services.AddSingleton<ICache<EolSubject, EolInfo>, Cache<EolSubject, EolInfo>>();

        services.AddScoped<IEndOfLifeService, EndOfLifeService>();

        services.AddSingleton<IEndOfLifeRefreshTrigger, EndOfLifeRefreshTrigger>();

        services.AddHttpClient<IEndOfLifeProvider, EndOfLifeClient>(client =>
        {
            client.DefaultRequestHeaders.UserAgent.ParseAdd(Constants.UserAgentName);
        });

        services.AddHostedService<EndOfLifeTimedHostedService>();

        Dictionary<string, bool> enabledReports = configuration.LoadEnabledTrivyReports();

        if (enabledReports.GetValueOrDefault(nameof(VulnerabilityReport)))
        {
            services.AddSingleton<
                IKubernetesEventProcessor<VulnerabilityReport, Digest>,
                EolSubjectCollector<VulnerabilityReport, Digest>>();
        }

        if (enabledReports.GetValueOrDefault(nameof(ClusterVulnerabilityReport)))
        {
            services.AddSingleton<
                IKubernetesEventProcessor<ClusterVulnerabilityReport, Uid>,
                EolSubjectCollector<ClusterVulnerabilityReport, Uid>>();
        }
    }
}
