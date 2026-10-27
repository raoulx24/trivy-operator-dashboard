using System.Net.Http.Headers;

namespace TrivyOperator.Dashboard.Infrastructure.Shared.Utils;

public static class HttpResponseMessageExtensions
{
    // Retry-After as a delay (delta seconds) or as a date; null when missing or already passed
    public static TimeSpan? GetRetryAfter(this HttpResponseMessage response)
    {
        RetryConditionHeaderValue? retryAfter = response.Headers.RetryAfter;

        if (retryAfter?.Delta is { } delta)
        {
            return delta;
        }

        if (retryAfter?.Date is { } date)
        {
            TimeSpan untilDate = date - DateTimeOffset.UtcNow;

            return untilDate > TimeSpan.Zero ? untilDate : null;
        }

        return null;
    }
}
