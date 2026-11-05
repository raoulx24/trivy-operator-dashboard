using TrivyOperator.Dashboard.Application.Shared.CveSubjects.Abstractions;
using TrivyOperator.Dashboard.Composition.Shared;
using TrivyOperator.Dashboard.Composition.Trivy;
using TrivyOperator.Dashboard.Domain.Shared.Details;
using TrivyOperator.Dashboard.Domain.Trivy.ValueObjects.Vulnerabilities;
using TrivyOperator.Dashboard.Infrastructure.Caching.ConcurrentCache.Abstractions;
using TrivyOperator.Dashboard.Infrastructure.Caching.InMemory.Details.Vulnerabilities;

namespace TrivyOperator.Dashboard.Composition.CveSubjects;

// shared by CVE Enrichment and NVD; when disabled nothing is registered - the consumers are disabled as well
public static class CveSubjectsServiceRegistrationExtensions
{
    // 1st level - main entrance
    public static void AddCveSubjectsRelatedServices(this IServiceCollection services, IConfiguration configuration)
    {
        CveSubjectsCompositionMode mode = CveSubjectsCompositionResolver.Resolve(configuration);

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

            case CveSubjectsCompositionMode.NoDefaultContext:
                CompositionLogger.Logger?.LogInformation(
                    "CVE Subjects related services are disabled - vulnerability reports are watched only in the default context");
                return;

            case CveSubjectsCompositionMode.Enabled:
                CompositionLogger.Logger?.LogInformation("Adding CVE Subjects related services");
                services.AddCveSubjectsServices();
                break;

            default:
                throw new ArgumentOutOfRangeException(nameof(mode), mode, null);
        }
    }

    // 2nd level - services registration
    private static void AddCveSubjectsServices(this IServiceCollection services)
    {
        // read from the vulnerability definitions store of the vulnerability reports (collected and swept there);
        // the store exists only when the vulnerability reports keep slim reports (default context) - no subjects otherwise
        services.AddSingleton<ICveSubjects>(sp =>
            new InMemoryCveSubjects(
                sp.GetKeyedService<IResourceDictionaryCache<VulnerabilityId, DetailEntry<VulnerabilityDefinition>>>(
                    DetailFamilies.Vulnerability)));
    }
}
