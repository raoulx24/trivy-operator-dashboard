using System.Net.Http.Headers;

namespace TrivyOperator.Dashboard.Infrastructure.CveEnrichment.Services;

internal static class CveHttpResponseExtensions
{
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
