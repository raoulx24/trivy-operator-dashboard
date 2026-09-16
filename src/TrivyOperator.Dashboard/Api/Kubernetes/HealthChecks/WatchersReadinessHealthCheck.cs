using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace TrivyOperator.Dashboard.Api.Kubernetes.HealthChecks;

public class WatchersReadinessHealthCheck : IHealthCheck
{
    public Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default
    ) =>
        // TODO: Add logic
        Task.FromResult(HealthCheckResult.Healthy("App is ready"));
}
