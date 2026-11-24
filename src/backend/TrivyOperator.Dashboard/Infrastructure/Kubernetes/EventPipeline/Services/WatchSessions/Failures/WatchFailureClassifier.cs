using k8s;
using k8s.Autorest;
using System.Globalization;
using System.Net.Sockets;
using TrivyOperator.Dashboard.Application.Kubernetes.Models;
using TrivyOperator.Dashboard.Infrastructure.Kubernetes.EventPipeline.Services.WatchSessions.Failures.Abstractions;

namespace TrivyOperator.Dashboard.Infrastructure.Kubernetes.EventPipeline.Services.WatchSessions.Failures;

// how failures surface in KubernetesClient (checked against v19.0.2):
// - non-2xx on a list call or on opening a watch: HttpOperationException, Response.StatusCode / Response.Headers
// - ERROR (Status) events inside a watch stream: KubernetesException via onError, Status.Code holds the HTTP code
// - transport failures mid-stream: thrown out of the enumeration (IOException / HttpRequestException family)
public sealed class WatchFailureClassifier : IWatchFailureClassifier
{
    private const string RetryAfterHeader = "Retry-After";

    public WatchFailure Classify(Exception exception, WatchPhase phase, int consecutiveFailures, DateTime failingSince)
    {
        (WatchFailureCategory category, int? statusCode, TimeSpan? retryAfter) = exception switch
        {
            HttpOperationException { Response: { } response } =>
                (FromStatusCode((int)response.StatusCode), (int)response.StatusCode, GetRetryAfter(response.Headers)),
            KubernetesException { Status.Code: { } code } kubernetesException =>
                (FromStatusCode(code), code, ToTimeSpan(kubernetesException.Status.Details?.RetryAfterSeconds)),
            // a Status without code; the message is the only hint left
            KubernetesException kubernetesException
                when kubernetesException.Message.Contains("too old resource version", StringComparison.OrdinalIgnoreCase) =>
                (WatchFailureCategory.Gone, 410, null),
            HttpRequestException { StatusCode: { } httpStatusCode } =>
                (FromStatusCode((int)httpStatusCode), (int)httpStatusCode, null),
            HttpRequestException or IOException or SocketException or TimeoutException =>
                (WatchFailureCategory.Transport, null, null),
            // not our cancellation (the caller filters it), so a timeout of the http client
            OperationCanceledException => (WatchFailureCategory.Transport, null, null),
            _ => (WatchFailureCategory.Unknown, (int?)null, (TimeSpan?)null),
        };

        return new WatchFailure(
            category,
            phase,
            statusCode,
            retryAfter,
            consecutiveFailures,
            failingSince,
            exception
        );
    }

    private static WatchFailureCategory FromStatusCode(int statusCode) => statusCode switch
    {
        410 => WatchFailureCategory.Gone,
        429 => WatchFailureCategory.Throttled,
        401 => WatchFailureCategory.Unauthorized,
        403 => WatchFailureCategory.Forbidden,
        404 => WatchFailureCategory.NotFound,
        408 or >= 500 => WatchFailureCategory.ServerTransient,
        _ => WatchFailureCategory.Unknown,
    };

    // Retry-After as delta seconds or as an HTTP date; null when missing, invalid or already passed
    private static TimeSpan? GetRetryAfter(IDictionary<string, IEnumerable<string>>? headers)
    {
        if (headers is null)
        {
            return null;
        }

        string? value = headers
            .FirstOrDefault(header => string.Equals(header.Key, RetryAfterHeader, StringComparison.OrdinalIgnoreCase))
            .Value?
            .FirstOrDefault();

        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        if (int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out int seconds))
        {
            return ToTimeSpan(seconds);
        }

        if (DateTimeOffset.TryParseExact(
                value,
                "r",
                CultureInfo.InvariantCulture,
                DateTimeStyles.AssumeUniversal,
                out DateTimeOffset date))
        {
            TimeSpan delay = date - DateTimeOffset.UtcNow;

            return delay > TimeSpan.Zero ? delay : null;
        }

        return null;
    }

    private static TimeSpan? ToTimeSpan(int? seconds) =>
        seconds is > 0 ? TimeSpan.FromSeconds(seconds.Value) : null;
}
