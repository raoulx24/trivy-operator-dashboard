using Microsoft.AspNetCore.Diagnostics.HealthChecks;

namespace TrivyOperator.Dashboard.Composition.Health;

public static class HealthEndpointExtensions
{
    public static void MapHealthEndpoints(
        this WebApplication app)
    {
        app.MapHealthChecks(
            "/healthz/live",
            new HealthCheckOptions
            {
                Predicate = check => check.Name == "watchers-liveness",
            });

        app.MapHealthChecks(
            "/healthz/ready",
            new HealthCheckOptions
            {
                Predicate = check => check.Name == "watchers-readiness",
            });
    }
}
