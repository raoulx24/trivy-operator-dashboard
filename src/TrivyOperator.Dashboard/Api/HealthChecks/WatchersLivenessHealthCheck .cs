using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;
using TrivyOperator.Dashboard.Application.K8sEventPipeline.Models.WatcherEvents;
using TrivyOperator.Dashboard.Application.K8sEventPipeline.Services.Options;
using TrivyOperator.Dashboard.Application.WatcherStates.Models;
using TrivyOperator.Dashboard.Infrastructure.Caching.ConcurrentCache.Abstractions;

namespace TrivyOperator.Dashboard.Api.HealthChecks;

public class WatchersLivenessHealthCheck(
    IConcurrentCache<WatcherKey, WatcherStateInfo> cache,
    IOptions<WatchersOptions> options,
    ILogger<WatchersLivenessHealthCheck> logger
) : IHealthCheck
{
    private readonly int timeFrameInSeconds = (int)((options.Value.WatchTimeoutInSeconds * 1.1) + 120);

    public Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default
    )
    {
        try
        {
            bool isAnyWatcherStale = cache.Select(kvp => kvp.Value)
                .Any(x => (DateTime.UtcNow - x.LastEventMoment).TotalSeconds > timeFrameInSeconds);
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
