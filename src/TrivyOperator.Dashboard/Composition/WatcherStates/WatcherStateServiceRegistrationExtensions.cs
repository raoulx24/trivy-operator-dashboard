using TrivyOperator.Dashboard.Application.K8sEventPipeline.Models.WatcherEvents;
using TrivyOperator.Dashboard.Application.K8sEventPipeline.Services.EventProcessors.Abstractions;
using TrivyOperator.Dashboard.Application.K8sEventPipeline.Services.Options;
using TrivyOperator.Dashboard.Application.Queries.WatcherStates.Services;
using TrivyOperator.Dashboard.Application.Queries.WatcherStates.Services.Abstractions;
using TrivyOperator.Dashboard.Application.WatcherStates.HostedServices;
using TrivyOperator.Dashboard.Application.WatcherStates.Models;
using TrivyOperator.Dashboard.Application.WatcherStates.Services;
using TrivyOperator.Dashboard.Composition.Common;
using TrivyOperator.Dashboard.Composition.Configuration;
using TrivyOperator.Dashboard.Domain.Trivy.Entities;
using TrivyOperator.Dashboard.Infrastructure.Caching.ConcurrentCache;
using TrivyOperator.Dashboard.Infrastructure.Caching.ConcurrentCache.Abstractions;
using TrivyOperator.Dashboard.Infrastructure.Trivy.Schema.ClusterComplianceReports;
using TrivyOperator.Dashboard.Infrastructure.Trivy.Schema.ConfigAuditReports;
using TrivyOperator.Dashboard.Infrastructure.Trivy.Schema.ExposedSecretReports;
using TrivyOperator.Dashboard.Infrastructure.Trivy.Schema.InfraAssessmentReports;
using TrivyOperator.Dashboard.Infrastructure.Trivy.Schema.RbacAssessmentReports;
using TrivyOperator.Dashboard.Infrastructure.Trivy.Schema.SbomReports;
using TrivyOperator.Dashboard.Infrastructure.Trivy.Schema.VulnerabilityReports.Models;

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
            IConcurrentCache<WatcherKey, WatcherStateInfo>, 
            ConcurrentCache<WatcherKey, WatcherStateInfo>>();

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
            IKubernetesEventProcessor<ClusterComplianceReportCr>,
            WatcherStateEventProcessor<ClusterComplianceReportCr>>();
    }

    if (enabledReports.GetValueOrDefault(nameof(ClusterInfraAssessmentReport)))
    {
        services.AddSingleton<
            IKubernetesEventProcessor<ClusterInfraAssessmentReportCr>,
            WatcherStateEventProcessor<ClusterInfraAssessmentReportCr>>();
    }

    if (enabledReports.GetValueOrDefault(nameof(ClusterRbacAssessmentReport)))
    {
        services.AddSingleton<
            IKubernetesEventProcessor<ClusterRbacAssessmentReportCr>,
            WatcherStateEventProcessor<ClusterRbacAssessmentReportCr>>();
    }

    if (enabledReports.GetValueOrDefault(nameof(ClusterSbomReport)))
    {
        services.AddSingleton<
            IKubernetesEventProcessor<ClusterSbomReportCr>,
            WatcherStateEventProcessor<ClusterSbomReportCr>>();
    }

    if (enabledReports.GetValueOrDefault(nameof(ClusterVulnerabilityReport)))
    {
        services.AddSingleton<
            IKubernetesEventProcessor<ClusterVulnerabilityReportCr>,
            WatcherStateEventProcessor<ClusterVulnerabilityReportCr>>();
    }

    if (enabledReports.GetValueOrDefault(nameof(ConfigAuditReport)))
    {
        services.AddSingleton<
            IKubernetesEventProcessor<ConfigAuditReportCr>,
            WatcherStateEventProcessor<ConfigAuditReportCr>>();
    }

    if (enabledReports.GetValueOrDefault(nameof(ExposedSecretReport)))
    {
        services.AddSingleton<
            IKubernetesEventProcessor<ExposedSecretReportCr>,
            WatcherStateEventProcessor<ExposedSecretReportCr>>();
    }

    if (enabledReports.GetValueOrDefault(nameof(InfraAssessmentReport)))
    {
        services.AddSingleton<
            IKubernetesEventProcessor<InfraAssessmentReportCr>,
            WatcherStateEventProcessor<InfraAssessmentReportCr>>();
    }

    if (enabledReports.GetValueOrDefault(nameof(RbacAssessmentReport)))
    {
        services.AddSingleton<
            IKubernetesEventProcessor<RbacAssessmentReportCr>,
            WatcherStateEventProcessor<RbacAssessmentReportCr>>();
    }

    if (enabledReports.GetValueOrDefault(nameof(SbomReport)))
    {
        services.AddSingleton<
            IKubernetesEventProcessor<SbomReportCr>,
            WatcherStateEventProcessor<SbomReportCr>>();
    }

    if (enabledReports.GetValueOrDefault(nameof(VulnerabilityReport)))
    {
        services.AddSingleton<
            IKubernetesEventProcessor<VulnerabilityReportCr>,
            WatcherStateEventProcessor<VulnerabilityReportCr>>();
    }
}

}
