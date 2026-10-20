using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;
using TrivyOperator.Dashboard.Application.Kubernetes.WatcherState.Models;
using TrivyOperator.Dashboard.Application.Kubernetes.WatcherState.Options;
using TrivyOperator.Dashboard.Application.Shared.Cache.Abstractions;

namespace TrivyOperator.Dashboard.Api.Kubernetes.HealthChecks;

public class WatchersLivenessHealthCheck(
    ICache<WatcherId, WatcherStateInfo> cache,
    IOptions<WatchersOptions> options,
    ILogger<WatchersLivenessHealthCheck> logger
) : IHealthCheck
{
    // Larger than the hosted service's recreate threshold (+60s), so a watcher gets a chance to be recreated before the app is reported unhealthy.
    private readonly TimeSpan timeFrame = TimeSpan.FromSeconds((options.Value.WatchTimeoutInSeconds * 1.1) + 120);

    public Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default
    )
    {
        try
        {
            bool isAnyWatcherStale = cache.Select(kvp => kvp.Value)
                .Any(x => x.Status == WatcherStateStatus.Green && DateTime.UtcNow - x.LastEventMoment > timeFrame);
            if (isAnyWatcherStale)
            {
                return Task.FromResult(HealthCheckResult.Unhealthy("Some watchers are stale."));
            }
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Error checking watchers liveness health check.");
        }

        return Task.FromResult(HealthCheckResult.Healthy("App is alive"));
    }
}
