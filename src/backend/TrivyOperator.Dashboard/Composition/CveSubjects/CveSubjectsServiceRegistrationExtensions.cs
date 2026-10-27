using TrivyOperator.Dashboard.Application.Kubernetes.EventPipeline.EventProcessors.Abstractions;
using TrivyOperator.Dashboard.Application.Shared.Cache.Abstractions;
using TrivyOperator.Dashboard.Application.Shared.CveSubjects.KubernetesEventProcessors;
using TrivyOperator.Dashboard.Application.Shared.CveSubjects.Options;
using TrivyOperator.Dashboard.Application.Shared.CveSubjects.Services;
using TrivyOperator.Dashboard.Composition.Configuration;
using TrivyOperator.Dashboard.Composition.Shared;
using TrivyOperator.Dashboard.Domain.Kubernetes.ValueObjects;
using TrivyOperator.Dashboard.Domain.Shared.ValueObjects;
using TrivyOperator.Dashboard.Domain.Trivy.Entities;
using TrivyOperator.Dashboard.Domain.Trivy.ValueObjects.Shared;
using TrivyOperator.Dashboard.Infrastructure.Caching.ConcurrentCache;

namespace TrivyOperator.Dashboard.Composition.CveSubjects;

// shared by CVE Enrichment and NVD; when disabled nothing is registered - the consumers are disabled as well
public static class CveSubjectsServiceRegistrationExtensions
{
    // 1st level - main entrance
    public static void AddCveSubjectsRelatedServices(this IServiceCollection services, IConfiguration configuration)
    {
        CveSubjectsCompositionMode mode = CveSubjectsCompositionResolver.Resolve(configuration);

        services.AddOptions<CveSubjectsOptions>().Bind(configuration.GetSection("CveSubjects"));

        switch (mode)
        {
            case CveSubjectsCompositionMode.NoConsumers:
                CompositionLogger.Logger?.LogInformation(
                    "CVE Subjects related services are disabled - neither CVE Enrichment nor NVD is enabled");
                return;

            case CveSubjectsCompositionMode.NoVulnerabilityReports:
                CompositionLogger.Logger?.LogInformation(
                    "CVE Subjects related services are disabled - no vulnerability reports are enabled");
                return;

            case CveSubjectsCompositionMode.Enabled:
                CompositionLogger.Logger?.LogInformation("Adding CVE Subjects related services");
                services.AddCveSubjectsServices(configuration);
                break;

            default:
                throw new ArgumentOutOfRangeException(nameof(mode), mode, null);
        }
    }

    // 2nd level - services registration
    private static void AddCveSubjectsServices(this IServiceCollection services, IConfiguration configuration)
    {
        // CVE -> newest scan timestamp of a report containing it
        services.AddSingleton<ICache<CveId, Timestamp>, Cache<CveId, Timestamp>>();

        services.AddHostedService<CveSubjectsHousekeepingHostedService>();

        Dictionary<string, bool> enabledReports = configuration.LoadEnabledTrivyReports();

        if (enabledReports.GetValueOrDefault(nameof(VulnerabilityReport)))
        {
            services.AddSingleton<
                IKubernetesEventProcessor<VulnerabilityReport, Digest>,
                CveSubjectCollector<VulnerabilityReport, Digest>>();
        }

        if (enabledReports.GetValueOrDefault(nameof(ClusterVulnerabilityReport)))
        {
            services.AddSingleton<
                IKubernetesEventProcessor<ClusterVulnerabilityReport, Uid>,
                CveSubjectCollector<ClusterVulnerabilityReport, Uid>>();
        }
    }
}
