using TrivyOperator.Dashboard.Api.K8s.HealthChecks;

namespace TrivyOperator.Dashboard.Composition.Health;

public static class HealthServiceRegistrationExtensions
{
    public static void AddHealthServices(this IServiceCollection services)
    {
        services
            .AddHealthChecks()
            .AddCheck<WatchersLivenessHealthCheck>("watchers-liveness")
            .AddCheck<WatchersReadinessHealthCheck>("watchers-readiness");
    }
}
