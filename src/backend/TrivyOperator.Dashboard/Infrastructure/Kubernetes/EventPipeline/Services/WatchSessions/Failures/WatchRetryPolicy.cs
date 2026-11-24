using Microsoft.Extensions.Options;
using TrivyOperator.Dashboard.Application.Kubernetes.Models;
using TrivyOperator.Dashboard.Application.Kubernetes.WatcherState.Options;
using TrivyOperator.Dashboard.Infrastructure.Kubernetes.EventPipeline.Services.WatchSessions.Failures.Abstractions;

namespace TrivyOperator.Dashboard.Infrastructure.Kubernetes.EventPipeline.Services.WatchSessions.Failures;

public sealed class WatchRetryPolicy(IOptions<WatchersOptions> options) : IWatchRetryPolicy
{
    public TimeSpan GetDelay(WatchFailure failure)
    {
        WatchersOptions watchersOptions = options.Value;

        TimeSpan delay = failure.Category switch
        {
            // the first 410 re-lists at once; a re-list that keeps hitting 410 backs off
            WatchFailureCategory.Gone when failure.ConsecutiveFailures <= 1 => TimeSpan.Zero,
            WatchFailureCategory.Forbidden or WatchFailureCategory.NotFound =>
                TimeSpan.FromSeconds(Math.Max(1, watchersOptions.PermanentFailureRetrySeconds)),
            _ => GetBackoff(failure.ConsecutiveFailures, watchersOptions),
        };

        return failure.RetryAfter is { } retryAfter && retryAfter > delay ? retryAfter : delay;
    }

    public bool IsRed(WatchFailure? failure)
    {
        if (failure is null)
        {
            return false;
        }

        WatchersOptions watchersOptions = options.Value;

        return failure.Category switch
        {
            WatchFailureCategory.Forbidden or WatchFailureCategory.NotFound => true,
            // a 410 is handled by re-listing, throttling is the server asking us to slow down
            WatchFailureCategory.Gone or WatchFailureCategory.Throttled => false,
            _ => failure.ConsecutiveFailures >= watchersOptions.RedAfterConsecutiveFailures
                 || DateTime.UtcNow - failure.FailingSince >= TimeSpan.FromSeconds(watchersOptions.RedAfterSeconds),
        };
    }

    // exponential backoff with "equal jitter": half of the step is fixed, the other half random,
    // so retries of sessions that failed together spread out, but never drop to (almost) zero
    private static TimeSpan GetBackoff(int attempt, WatchersOptions watchersOptions)
    {
        double baseSeconds = Math.Max(1, watchersOptions.RetryBaseDelaySeconds);
        double maxSeconds = Math.Max(baseSeconds, watchersOptions.RetryMaxDelaySeconds);

        int exponent = Math.Clamp(attempt - 1, 0, 30);
        double stepSeconds = Math.Min(maxSeconds, baseSeconds * Math.Pow(2, exponent));

        double half = stepSeconds / 2;

        return TimeSpan.FromSeconds(half + Random.Shared.NextDouble() * half);
    }
}
