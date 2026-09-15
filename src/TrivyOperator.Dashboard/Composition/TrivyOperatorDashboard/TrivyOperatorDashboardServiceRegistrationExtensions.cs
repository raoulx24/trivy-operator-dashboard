using TrivyOperator.Dashboard.Composition.Alerts;
using TrivyOperator.Dashboard.Composition.BackendSettings;
using TrivyOperator.Dashboard.Composition.GitHub;
using TrivyOperator.Dashboard.Composition.History;
using TrivyOperator.Dashboard.Composition.Kubernetes;
using TrivyOperator.Dashboard.Composition.Trivy;
using TrivyOperator.Dashboard.Composition.TrivyDependencies;
using TrivyOperator.Dashboard.Composition.WatcherStates;

namespace TrivyOperator.Dashboard.Composition.TrivyOperatorDashboard;

public static class TrivyOperatorDashboardServiceRegistrationExtensions
{
    public static void AddTrivyOperatorDashboardServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddAlertsRelatedServices();
        services.AddBackendSettingsRelatedServices(configuration);
        services.AddGitHubRelatedServices(configuration);
        services.AddKubernetesRelatedServices(configuration);
        services.AddTrivyReportRelatedServices(configuration);
        services.AddTrivyDependenciesRelatedServices(configuration);
        services.AddWatcherStateRelatedServices(configuration);
        services.AddHistoryRelatedServices(configuration);
    }
}
