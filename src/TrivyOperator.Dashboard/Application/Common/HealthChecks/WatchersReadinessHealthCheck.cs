using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace TrivyOperator.Dashboard.Application.Common.HealthChecks;

public class WatchersReadinessHealthCheck : IHealthCheck
{
    public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        // TODO: Add logic
        return Task.FromResult(HealthCheckResult.Healthy("App is ready"));
    }
}
