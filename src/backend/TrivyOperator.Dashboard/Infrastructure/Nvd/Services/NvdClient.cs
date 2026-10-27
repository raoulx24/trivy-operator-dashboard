using System.Globalization;
using System.Net;
using System.Text.Json;
using Microsoft.Extensions.Options;
using TrivyOperator.Dashboard.Application.Nvd.Abstractions;
using TrivyOperator.Dashboard.Application.Nvd.Options;
using TrivyOperator.Dashboard.Domain.Nvd.Entities;
using TrivyOperator.Dashboard.Domain.Shared.ValueObjects;
using TrivyOperator.Dashboard.Infrastructure.Nvd.Mappers;
using TrivyOperator.Dashboard.Infrastructure.Nvd.Models;
using TrivyOperator.Dashboard.Infrastructure.Nvd.Serializations;
using TrivyOperator.Dashboard.Infrastructure.Shared.Utils;

namespace TrivyOperator.Dashboard.Infrastructure.Nvd.Services;

// failures are returned as results, not logged as errors - the user sees them in the dialog
// singleton (used by the NVD service, which holds the in-flight requests) - so a named client per call, not a typed one
// checked 2026-10-27:
// - an unknown CVE is 200 with totalResults 0 (NotListed)
// - an invalid cveId is 404, with the reason in the "message" response header
// - rate limited requests are 403 (or 503), usually without Retry-After
public class NvdClient(
    IHttpClientFactory httpClientFactory,
    NvdRateLimiter rateLimiter,
    IOptionsMonitor<NvdOptions> options,
    ILogger<NvdClient> logger) : INvdProvider
{
    public const string HttpClientName = "Nvd";

    private const string ApiKeyHeader = "apiKey";
    private const int PermitLimitWithoutApiKey = 5;
    private const int PermitLimitWithApiKey = 50;
    private static readonly TimeSpan RateLimitWindow = TimeSpan.FromSeconds(30);

    public async Task<CveSourceResult<NvdInfo>> GetCve(CveId id, CancellationToken ctx = default)
    {
        NvdOptions nvdOptions = options.CurrentValue;
        string? apiKey = string.IsNullOrWhiteSpace(nvdOptions.ApiKey) ? null : nvdOptions.ApiKey.Trim();

        if (!id.IsValid)
        {
            // the original value is gone (CveId keeps the sentinel), so it cannot be named
            return Failed("Not a CVE id");
        }

        int permitLimit = apiKey is null ? PermitLimitWithoutApiKey : PermitLimitWithApiKey;

        if (!rateLimiter.TryAcquire(permitLimit, RateLimitWindow, out TimeSpan retryAfter))
        {
            return Failed(
                $"NVD rate limit reached ({permitLimit} requests per {RateLimitWindow.TotalSeconds:0}s" +
                $"{(apiKey is null ? " without an api key" : string.Empty)}) - " +
                $"retry in {Math.Ceiling(retryAfter.TotalSeconds):0}s");
        }

        string url = $"{nvdOptions.BaseUrl.TrimEnd('/')}?cveId={Uri.EscapeDataString(id.Value)}";

        using CancellationTokenSource timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(ctx);
        timeoutCts.CancelAfter(TimeSpan.FromSeconds(Math.Max(1, nvdOptions.RequestTimeoutInSeconds)));

        try
        {
            using HttpRequestMessage request = new(HttpMethod.Get, url);

            if (apiKey is not null)
            {
                request.Headers.Add(ApiKeyHeader, apiKey);
            }

            HttpClient httpClient = httpClientFactory.CreateClient(HttpClientName);
            using HttpResponseMessage response = await httpClient.SendAsync(request, timeoutCts.Token);

            if (!response.IsSuccessStatusCode)
            {
                return Failed(response.ToNvdErrorMessage(id));
            }

            await using Stream content = await response.Content.ReadAsStreamAsync(timeoutCts.Token);
            NvdCveResponse? nvdResponse = await JsonSerializer.DeserializeAsync(
                content,
                NvdJsonContext.Default.NvdCveResponse,
                timeoutCts.Token);

            if (nvdResponse is null)
            {
                return Failed($"NVD returned an empty or unexpected response for {id}");
            }

            if (nvdResponse.TotalResults == 0)
            {
                return CveSourceResult<NvdInfo>.NotListed(Timestamp.Now());
            }

            NvdInfo? info = nvdResponse.ToDomain(id);

            return info is null
                ? Failed($"NVD returned no record for {id}")
                : CveSourceResult<NvdInfo>.Ok(info, Timestamp.Now());
        }
        catch (OperationCanceledException) when (ctx.IsCancellationRequested)
        {
            throw;
        }
        catch (OperationCanceledException)
        {
            return Failed($"Timeout while calling NVD for {id}");
        }
        catch (Exception ex) when (ex is HttpRequestException or JsonException)
        {
            logger.LogDebug(ex, "Error fetching {cveId} from NVD", id.Value);

            return Failed($"Error calling NVD for {id} - {ex.Message}");
        }
    }

    private static CveSourceResult<NvdInfo> Failed(string errorMessage) =>
        CveSourceResult<NvdInfo>.Failed(null, errorMessage, Timestamp.Now());
}

internal static class NvdHttpResponseExtensions
{
    private const string MessageHeader = "message";

    public static string ToNvdErrorMessage(this HttpResponseMessage response, CveId id)
    {
        string status = $"{(int)response.StatusCode} {response.ReasonPhrase}";
        string? message = response.Headers.TryGetValues(MessageHeader, out IEnumerable<string>? values)
            ? string.Join("; ", values)
            : null;
        string details = message is null ? string.Empty : $" - {message}";
        string retry = response.GetRetryAfter() is { } retryAfter
            ? $" - retry in {Math.Ceiling(retryAfter.TotalSeconds).ToString(CultureInfo.InvariantCulture)}s"
            : string.Empty;

        return response.StatusCode switch
        {
            HttpStatusCode.Forbidden or HttpStatusCode.TooManyRequests or HttpStatusCode.ServiceUnavailable =>
                $"NVD refused the request for {id} ({status}{details}) - rate limited, busy or invalid api key{retry}",
            _ => $"NVD returned {status} for {id}{details}",
        };
    }
}
