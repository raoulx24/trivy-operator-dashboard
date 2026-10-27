using System.Net;
using System.Text.Json;
using Microsoft.Extensions.Options;
using TrivyOperator.Dashboard.Application.CveEnrichment.Abstractions;
using TrivyOperator.Dashboard.Application.CveEnrichment.Models;
using TrivyOperator.Dashboard.Application.CveEnrichment.Options;
using TrivyOperator.Dashboard.Domain.CveEnrichment.Entities;
using TrivyOperator.Dashboard.Domain.Shared.ValueObjects;
using TrivyOperator.Dashboard.Infrastructure.CveEnrichment.Mappers;
using TrivyOperator.Dashboard.Infrastructure.CveEnrichment.Models;
using TrivyOperator.Dashboard.Infrastructure.CveEnrichment.Serializations;
using TrivyOperator.Dashboard.Infrastructure.Shared.Utils;

namespace TrivyOperator.Dashboard.Infrastructure.CveEnrichment.Services;

// failures are returned as results, not logged as errors - the caller decides how often to report them
// two quirks of api.first.org (checked 2026-10-26) shape the requests:
// - an empty cve parameter returns the whole database, so no request is sent for an empty batch
// - a cve parameter longer than ~2000 chars silently returns no data (as if no CVE was scored),
//   so the batch is split into requests with a shorter cve parameter
public class EpssClient(
    HttpClient httpClient,
    IOptionsMonitor<CveEnrichmentOptions> options,
    ILogger<EpssClient> logger) : IEpssProvider
{
    private const int MaxCveParameterLength = 1900;

    public async Task<CveProviderResult<EpssScore>> GetScores(
        string baseUrl,
        IReadOnlyCollection<CveId> ids,
        CancellationToken ctx = default)
    {
        List<EpssScore> scores = [];

        foreach (IReadOnlyList<CveId> chunk in Chunk(ids.Where(static x => x.IsValid).Distinct()))
        {
            CveProviderResult<EpssScore> result = await GetChunk(baseUrl, chunk, ctx);

            if (result.Status != CveProviderStatuses.Ok)
            {
                return result;
            }

            scores.AddRange(result.Items ?? []);
        }

        return CveProviderResult<EpssScore>.Ok(scores);
    }

    private async Task<CveProviderResult<EpssScore>> GetChunk(
        string baseUrl,
        IReadOnlyList<CveId> ids,
        CancellationToken ctx)
    {
        string cveParameter = string.Join(',', ids.Select(static x => x.Value));
        // limit is set explicitly - the api default (100) would cut a larger chunk
        string url = $"{baseUrl.TrimEnd('/')}/epss?cve={cveParameter}&limit={ids.Count}";

        using CancellationTokenSource timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(ctx);
        timeoutCts.CancelAfter(TimeSpan.FromSeconds(Math.Max(1, options.CurrentValue.Epss.RequestTimeoutInSeconds)));

        try
        {
            using HttpResponseMessage response = await httpClient.GetAsync(url, timeoutCts.Token);

            if (response.StatusCode == HttpStatusCode.TooManyRequests)
            {
                return CveProviderResult<EpssScore>.Error($"Rate limited by {baseUrl}", response.GetRetryAfter());
            }

            if (!response.IsSuccessStatusCode)
            {
                return CveProviderResult<EpssScore>.Error(
                    $"{baseUrl} returned {(int)response.StatusCode} {response.ReasonPhrase}");
            }

            await using Stream content = await response.Content.ReadAsStreamAsync(timeoutCts.Token);
            EpssResponse? epssResponse = await JsonSerializer.DeserializeAsync(
                content,
                CveEnrichmentJsonContext.Default.EpssResponse,
                timeoutCts.Token);

            if (epssResponse?.Data is null)
            {
                return CveProviderResult<EpssScore>.Error($"{baseUrl} returned an empty or unexpected response");
            }

            return CveProviderResult<EpssScore>.Ok(epssResponse.ToDomain());
        }
        catch (OperationCanceledException) when (ctx.IsCancellationRequested)
        {
            throw;
        }
        catch (OperationCanceledException)
        {
            return CveProviderResult<EpssScore>.Error($"Timeout while calling {baseUrl}");
        }
        catch (Exception ex) when (ex is HttpRequestException or JsonException)
        {
            logger.LogDebug(ex, "Error fetching {url}", url);

            return CveProviderResult<EpssScore>.Error($"Error calling {baseUrl} - {ex.Message}");
        }
    }

    private static IEnumerable<IReadOnlyList<CveId>> Chunk(IEnumerable<CveId> ids)
    {
        List<CveId> chunk = [];
        int length = 0;

        foreach (CveId id in ids)
        {
            int addedLength = id.Value.Length + (chunk.Count == 0 ? 0 : 1);

            if (chunk.Count > 0 && length + addedLength > MaxCveParameterLength)
            {
                yield return chunk;

                chunk = [];
                length = 0;
                addedLength = id.Value.Length;
            }

            chunk.Add(id);
            length += addedLength;
        }

        if (chunk.Count > 0)
        {
            yield return chunk;
        }
    }
}
