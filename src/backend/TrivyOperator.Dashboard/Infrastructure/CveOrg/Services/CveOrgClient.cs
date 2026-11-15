using System.Globalization;
using System.Net;
using System.Text.Json;
using Microsoft.Extensions.Options;
using TrivyOperator.Dashboard.Application.CveDetails.Abstractions;
using TrivyOperator.Dashboard.Application.CveDetails.Options;
using TrivyOperator.Dashboard.Domain.CveDetails.Entities;
using TrivyOperator.Dashboard.Domain.Shared.ValueObjects;
using TrivyOperator.Dashboard.Infrastructure.CveOrg.Mappers;
using TrivyOperator.Dashboard.Infrastructure.CveOrg.Models;
using TrivyOperator.Dashboard.Infrastructure.CveOrg.Serializations;
using TrivyOperator.Dashboard.Infrastructure.Shared.Utils;

namespace TrivyOperator.Dashboard.Infrastructure.CveOrg.Services;

// failures are returned as results, not logged as errors - the user sees them in the dialog
// singleton (used by the CVE details service, which holds the in-flight requests) - so a named client per call
// checked 2026-11-15:
// - no api key; the service sends RateLimit-Limit/-Remaining/-Reset headers (25000 requests per 60s)
// - an unknown CVE is 404 (error CVE_RECORD_DNE) - NotListed
// - a rejected CVE is 200, with cveMetadata.state REJECTED - NotListed as well
// - an invalid cveId is 400 (BAD_INPUT)
public class CveOrgClient(
    IHttpClientFactory httpClientFactory,
    IOptionsMonitor<CveOrgOptions> options,
    ILogger<CveOrgClient> logger) : ICveDetailsProvider
{
    public const string HttpClientName = "CveOrg";

    public async Task<CveSourceResult<CveDetailsInfo>> GetCve(CveId id, CancellationToken ctx = default)
    {
        CveOrgOptions cveOrgOptions = options.CurrentValue;

        if (!id.IsValid)
        {
            // the original value is gone (CveId keeps the sentinel), so it cannot be named
            return Failed("Not a CVE id");
        }

        string url = $"{cveOrgOptions.BaseUrl.TrimEnd('/')}/{Uri.EscapeDataString(id.Value)}";

        using CancellationTokenSource timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(ctx);
        timeoutCts.CancelAfter(TimeSpan.FromSeconds(Math.Max(1, cveOrgOptions.RequestTimeoutInSeconds)));

        try
        {
            using HttpRequestMessage request = new(HttpMethod.Get, url);

            HttpClient httpClient = httpClientFactory.CreateClient(HttpClientName);
            using HttpResponseMessage response = await httpClient.SendAsync(request, timeoutCts.Token);

            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                return CveSourceResult<CveDetailsInfo>.NotListed(Timestamp.Now());
            }

            if (!response.IsSuccessStatusCode)
            {
                return Failed(response.ToCveOrgErrorMessage(id));
            }

            await using Stream content = await response.Content.ReadAsStreamAsync(timeoutCts.Token);
            CveOrgRecordResponse? record = await JsonSerializer.DeserializeAsync(
                content,
                CveOrgJsonContext.Default.CveOrgRecordResponse,
                timeoutCts.Token);

            if (record is null)
            {
                return Failed($"CVE.org returned an empty or unexpected response for {id}");
            }

            // not published (rejected) or not the requested record
            CveDetailsInfo? info = record.ToDomain(id);

            return info is null
                ? CveSourceResult<CveDetailsInfo>.NotListed(Timestamp.Now())
                : CveSourceResult<CveDetailsInfo>.Ok(info, Timestamp.Now());
        }
        catch (OperationCanceledException) when (ctx.IsCancellationRequested)
        {
            throw;
        }
        catch (OperationCanceledException)
        {
            return Failed($"Timeout while calling CVE.org for {id}");
        }
        catch (Exception ex) when (ex is HttpRequestException or JsonException)
        {
            logger.LogDebug(ex, "Error fetching {cveId} from CVE.org", id.Value);

            return Failed($"Error calling CVE.org for {id} - {ex.Message}");
        }
    }

    private static CveSourceResult<CveDetailsInfo> Failed(string errorMessage) =>
        CveSourceResult<CveDetailsInfo>.Failed(null, errorMessage, Timestamp.Now());
}

internal static class CveOrgHttpResponseExtensions
{
    private const string RateLimitResetHeader = "RateLimit-Reset";

    public static string ToCveOrgErrorMessage(this HttpResponseMessage response, CveId id)
    {
        string status = $"{(int)response.StatusCode} {response.ReasonPhrase}";
        TimeSpan? retryAfter = response.GetRetryAfter() ?? response.GetRateLimitReset();
        string retry = retryAfter is { } wait
            ? $" - retry in {Math.Ceiling(wait.TotalSeconds).ToString(CultureInfo.InvariantCulture)}s"
            : string.Empty;

        return response.StatusCode switch
        {
            HttpStatusCode.TooManyRequests or HttpStatusCode.ServiceUnavailable =>
                $"CVE.org refused the request for {id} ({status}) - rate limited or busy{retry}",
            _ => $"CVE.org returned {status} for {id}",
        };
    }

    // seconds until the rate limit window resets
    private static TimeSpan? GetRateLimitReset(this HttpResponseMessage response) =>
        response.Headers.TryGetValues(RateLimitResetHeader, out IEnumerable<string>? values) &&
        double.TryParse(values.FirstOrDefault(), NumberStyles.Float, CultureInfo.InvariantCulture, out double seconds) &&
        seconds >= 0
            ? TimeSpan.FromSeconds(seconds)
            : null;
}
