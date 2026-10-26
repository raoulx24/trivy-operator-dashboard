using System.Globalization;
using System.Net;
using System.Text.Json;
using Microsoft.Extensions.Options;
using TrivyOperator.Dashboard.Application.CveEnrichment.Abstractions;
using TrivyOperator.Dashboard.Application.CveEnrichment.Models;
using TrivyOperator.Dashboard.Application.CveEnrichment.Options;
using TrivyOperator.Dashboard.Domain.CveEnrichment.Entities;
using TrivyOperator.Dashboard.Infrastructure.CveEnrichment.Mappers;
using TrivyOperator.Dashboard.Infrastructure.CveEnrichment.Models;
using TrivyOperator.Dashboard.Infrastructure.CveEnrichment.Serializations;

namespace TrivyOperator.Dashboard.Infrastructure.CveEnrichment.Services;

// failures are returned as results, not logged as errors - the caller decides how often to report them
// the validator is the Last-Modified of the feed: cisa.gov ignores If-None-Match (always 200),
// but answers If-Modified-Since with a 304 (checked 2026-10-26), so an unchanged catalog is not downloaded again
public class KevClient(
    HttpClient httpClient,
    IOptionsMonitor<CveEnrichmentOptions> options,
    ILogger<KevClient> logger) : IKevProvider
{
    public async Task<CveProviderResult<KevEntry>> GetCatalog(
        string url,
        string? validator,
        CancellationToken ctx = default)
    {
        using CancellationTokenSource timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(ctx);
        timeoutCts.CancelAfter(TimeSpan.FromSeconds(Math.Max(1, options.CurrentValue.Kev.RequestTimeoutInSeconds)));

        try
        {
            using HttpRequestMessage request = new(HttpMethod.Get, url);

            if (DateTimeOffset.TryParse(
                    validator,
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.AssumeUniversal,
                    out DateTimeOffset lastModified))
            {
                request.Headers.IfModifiedSince = lastModified;
            }

            using HttpResponseMessage response = await httpClient.SendAsync(request, timeoutCts.Token);

            switch (response.StatusCode)
            {
                case HttpStatusCode.NotModified:
                    return CveProviderResult<KevEntry>.NotModified();
                case HttpStatusCode.TooManyRequests:
                    return CveProviderResult<KevEntry>.Error($"Rate limited by {url}", response.GetRetryAfter());
            }

            if (!response.IsSuccessStatusCode)
            {
                return CveProviderResult<KevEntry>.Error(
                    $"{url} returned {(int)response.StatusCode} {response.ReasonPhrase}");
            }

            await using Stream content = await response.Content.ReadAsStreamAsync(timeoutCts.Token);
            KevCatalogResponse? catalog = await JsonSerializer.DeserializeAsync(
                content,
                CveEnrichmentJsonContext.Default.KevCatalogResponse,
                timeoutCts.Token);

            // an empty catalog would mark every CVE as not listed, so it is treated as an error
            if (catalog?.Vulnerabilities is not { Length: > 0, })
            {
                return CveProviderResult<KevEntry>.Error($"{url} returned an empty or unexpected response");
            }

            IReadOnlyList<KevEntry> entries = catalog.ToDomain();

            logger.LogDebug(
                "KEV catalog {catalogVersion} fetched - {count} entries, {mapped} mapped",
                catalog.CatalogVersion,
                catalog.Vulnerabilities.Length,
                entries.Count);

            return CveProviderResult<KevEntry>.Ok(
                entries,
                response.Content.Headers.LastModified?.ToString("R", CultureInfo.InvariantCulture));
        }
        catch (OperationCanceledException) when (ctx.IsCancellationRequested)
        {
            throw;
        }
        catch (OperationCanceledException)
        {
            return CveProviderResult<KevEntry>.Error($"Timeout while calling {url}");
        }
        catch (Exception ex) when (ex is HttpRequestException or JsonException)
        {
            logger.LogDebug(ex, "Error fetching {url}", url);

            return CveProviderResult<KevEntry>.Error($"Error calling {url} - {ex.Message}");
        }
    }
}
