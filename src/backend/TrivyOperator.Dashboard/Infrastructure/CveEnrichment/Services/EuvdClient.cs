using System.Net;
using System.Text.Json;
using Microsoft.Extensions.Options;
using TrivyOperator.Dashboard.Application.CveEnrichment.Abstractions;
using TrivyOperator.Dashboard.Application.CveEnrichment.Models;
using TrivyOperator.Dashboard.Application.CveEnrichment.Options;
using TrivyOperator.Dashboard.Domain.CveEnrichment.Entities;
using TrivyOperator.Dashboard.Domain.CveEnrichment.ValueObjects;
using TrivyOperator.Dashboard.Infrastructure.CveEnrichment.Mappers;
using TrivyOperator.Dashboard.Infrastructure.CveEnrichment.Models;
using TrivyOperator.Dashboard.Infrastructure.CveEnrichment.Serializations;

namespace TrivyOperator.Dashboard.Infrastructure.CveEnrichment.Services;

// failures are returned as results, not logged as errors - the caller decides how often to report them
// /exploitedvulnerabilities returns only the latest few entries, so the exploited list is read through /search
public class EuvdClient(
    HttpClient httpClient,
    IOptionsMonitor<CveEnrichmentOptions> options,
    ILogger<EuvdClient> logger) : IEuvdProvider
{
    // safety net against a paging that never ends
    private const int MaxPages = 1000;

    public async Task<CveProviderResult<EuvdEntry>> GetExploited(
        string baseUrl,
        int pageSize,
        CancellationToken ctx = default)
    {
        int size = Math.Clamp(pageSize, 1, 100);
        // the same CVE may be an alias of more EUVD entries - the earliest exploitedSince wins
        Dictionary<CveId, EuvdEntry> entries = [];
        int read = 0;

        for (int page = 0; page < MaxPages; page++)
        {
            string url = $"{baseUrl.TrimEnd('/')}/search?exploited=true&page={page}&size={size}";
            (EuvdSearchResponse? response, CveProviderResult<EuvdEntry>? error) = await GetPage(url, baseUrl, ctx);

            if (error is not null)
            {
                return error;
            }

            EuvdItem[] items = response!.Items!;

            foreach (EuvdEntry entry in items.SelectMany(static x => x.ToDomain()))
            {
                if (!entries.TryGetValue(entry.Id, out EuvdEntry? existing) || IsEarlier(entry, existing))
                {
                    entries[entry.Id] = entry;
                }
            }

            read += items.Length;

            if (items.Length == 0 || read >= (response.Total ?? 0))
            {
                // an empty list would mark every CVE as not listed, so it is treated as an error
                return entries.Count == 0
                    ? CveProviderResult<EuvdEntry>.Error($"{baseUrl} returned no exploited vulnerabilities")
                    : CveProviderResult<EuvdEntry>.Ok([.. entries.Values,]);
            }
        }

        return CveProviderResult<EuvdEntry>.Error($"{baseUrl} returned more than {MaxPages} pages");
    }

    private async Task<(EuvdSearchResponse? Response, CveProviderResult<EuvdEntry>? Error)> GetPage(
        string url,
        string baseUrl,
        CancellationToken ctx)
    {
        using CancellationTokenSource timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(ctx);
        timeoutCts.CancelAfter(TimeSpan.FromSeconds(Math.Max(1, options.CurrentValue.Euvd.RequestTimeoutInSeconds)));

        try
        {
            using HttpResponseMessage response = await httpClient.GetAsync(url, timeoutCts.Token);

            if (response.StatusCode == HttpStatusCode.TooManyRequests)
            {
                return (null, CveProviderResult<EuvdEntry>.Error($"Rate limited by {baseUrl}", response.GetRetryAfter()));
            }

            if (!response.IsSuccessStatusCode)
            {
                return (null, CveProviderResult<EuvdEntry>.Error(
                    $"{url} returned {(int)response.StatusCode} {response.ReasonPhrase}"));
            }

            await using Stream content = await response.Content.ReadAsStreamAsync(timeoutCts.Token);
            EuvdSearchResponse? searchResponse = await JsonSerializer.DeserializeAsync(
                content,
                CveEnrichmentJsonContext.Default.EuvdSearchResponse,
                timeoutCts.Token);

            return searchResponse?.Items is null
                ? (null, CveProviderResult<EuvdEntry>.Error($"{url} returned an empty or unexpected response"))
                : (searchResponse, null);
        }
        catch (OperationCanceledException) when (ctx.IsCancellationRequested)
        {
            throw;
        }
        catch (OperationCanceledException)
        {
            return (null, CveProviderResult<EuvdEntry>.Error($"Timeout while calling {url}"));
        }
        catch (Exception ex) when (ex is HttpRequestException or JsonException)
        {
            logger.LogDebug(ex, "Error fetching {url}", url);

            return (null, CveProviderResult<EuvdEntry>.Error($"Error calling {url} - {ex.Message}"));
        }
    }

    private static bool IsEarlier(EuvdEntry candidate, EuvdEntry existing) =>
        candidate.ExploitedSince is { } candidateSince &&
        (existing.ExploitedSince is not { } existingSince || candidateSince < existingSince);
}
