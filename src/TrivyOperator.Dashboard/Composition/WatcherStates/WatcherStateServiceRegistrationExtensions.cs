using TrivyOperator.Dashboard.Application.Kubernetes.EventPipeline.EventProcessors.Abstractions;
using TrivyOperator.Dashboard.Application.Kubernetes.WatcherState.Abstractions;
using TrivyOperator.Dashboard.Application.Kubernetes.WatcherState.Models;
using TrivyOperator.Dashboard.Application.Kubernetes.WatcherState.Options;
using TrivyOperator.Dashboard.Application.Queries.WatcherStates.Services;
using TrivyOperator.Dashboard.Application.Queries.WatcherStates.Services.Abstractions;
using TrivyOperator.Dashboard.Application.Shared.Cache.Abstractions;
using TrivyOperator.Dashboard.Composition.Configuration;
using TrivyOperator.Dashboard.Composition.Shared;
using TrivyOperator.Dashboard.Domain.Kubernetes.ValueObjects;
using TrivyOperator.Dashboard.Domain.Trivy.Entities;
using TrivyOperator.Dashboard.Domain.Trivy.ValueObjects.Shared;
using TrivyOperator.Dashboard.Infrastructure.Caching.ConcurrentCache;
using TrivyOperator.Dashboard.Infrastructure.Kubernetes.WatcherStates.HostedServices;
using TrivyOperator.Dashboard.Infrastructure.Kubernetes.WatcherStates.Services;

namespace TrivyOperator.Dashboard.Composition.WatcherStates;

public static class WatcherStateServiceRegistrationExtensions
{
    // 1st level - main entrance
    public static void AddWatcherStateRelatedServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        WatcherStateCompositionMode mode = WatcherStateCompositionResolver.Resolve(configuration);
        
        switch (mode)
        {
            case WatcherStateCompositionMode.Disabled:
                CompositionLogger.Logger?.LogInformation("Watcher State related services are disabled");
                return;

            case WatcherStateCompositionMode.Enabled:
                CompositionLogger.Logger?.LogInformation("Adding Watcher State related services");
                services.AddWatcherStateServices(configuration);
                break;

            default:
                throw new ArgumentOutOfRangeException(nameof(mode), mode, null);
        }
    }

    // 2nd level - services registration
    private static void AddWatcherStateServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<WatchersOptions>(configuration.GetSection("Watchers"));
        
        Dictionary<string, bool> enabledReports = configuration.LoadEnabledTrivyReports();

        services.AddSingleton<
            ICache<ResourceLocation, WatcherStateInfo>, 
            Cache<ResourceLocation, WatcherStateInfo>>();

        services.AddScoped<IKnownKubernetesTypeFactory, IKnownKubernetesTypeFactory>();
        
        services.AddScoped<IWatcherStatusService, WatcherStatusService>();

        services.AddHostedService<WatcherStateCacheTimedHostedService>();

        AddWatcherStateEventProcessors(services, enabledReports);
    }

    // 3rd level - helpers
    private static void AddWatcherStateEventProcessors(IServiceCollection services, IReadOnlyDictionary<string, bool> enabledReports)
{
    if (enabledReports.GetValueOrDefault(nameof(ClusterComplianceReport)))
    {
        services.AddSingleton<
            IKubernetesEventProcessor<ClusterComplianceReport, Uid>,
            WatcherStateEventProcessor<ClusterComplianceReport, Uid>>();
    }

    if (enabledReports.GetValueOrDefault(nameof(ClusterInfraAssessmentReport)))
    {
        services.AddSingleton<
            IKubernetesEventProcessor<ClusterInfraAssessmentReport, Uid>,
            WatcherStateEventProcessor<ClusterInfraAssessmentReport, Uid>>();
    }

    if (enabledReports.GetValueOrDefault(nameof(ClusterRbacAssessmentReport)))
    {
        services.AddSingleton<
            IKubernetesEventProcessor<ClusterRbacAssessmentReport, Uid>,
            WatcherStateEventProcessor<ClusterRbacAssessmentReport, Uid>>();
    }

    if (enabledReports.GetValueOrDefault(nameof(ClusterSbomReport)))
    {
        services.AddSingleton<
            IKubernetesEventProcessor<ClusterSbomReport, Uid>,
            WatcherStateEventProcessor<ClusterSbomReport, Uid>>();
    }

    if (enabledReports.GetValueOrDefault(nameof(ClusterVulnerabilityReport)))
    {
        services.AddSingleton<
            IKubernetesEventProcessor<ClusterVulnerabilityReport, Uid>,
            WatcherStateEventProcessor<ClusterVulnerabilityReport, Uid>>();
    }

    if (enabledReports.GetValueOrDefault(nameof(ConfigAuditReport)))
    {
        services.AddSingleton<
            IKubernetesEventProcessor<ConfigAuditReport, Uid>,
            WatcherStateEventProcessor<ConfigAuditReport, Uid>>();
    }

    if (enabledReports.GetValueOrDefault(nameof(ExposedSecretReport)))
    {
        services.AddSingleton<
            IKubernetesEventProcessor<ExposedSecretReport, Digest>,
            WatcherStateEventProcessor<ExposedSecretReport, Digest>>();
    }

    if (enabledReports.GetValueOrDefault(nameof(InfraAssessmentReport)))
    {
        services.AddSingleton<
            IKubernetesEventProcessor<InfraAssessmentReport, Uid>,
            WatcherStateEventProcessor<InfraAssessmentReport, Uid>>();
    }

    if (enabledReports.GetValueOrDefault(nameof(RbacAssessmentReport)))
    {
        services.AddSingleton<
            IKubernetesEventProcessor<RbacAssessmentReport, Uid>,
            WatcherStateEventProcessor<RbacAssessmentReport, Uid>>();
    }

    if (enabledReports.GetValueOrDefault(nameof(SbomReport)))
    {
        services.AddSingleton<
            IKubernetesEventProcessor<SbomReport, Digest>,
            WatcherStateEventProcessor<SbomReport, Digest>>();
    }

    if (enabledReports.GetValueOrDefault(nameof(VulnerabilityReport)))
    {
        services.AddSingleton<
            IKubernetesEventProcessor<VulnerabilityReport, Digest>,
            WatcherStateEventProcessor<VulnerabilityReport, Digest>>();
    }
}

}
