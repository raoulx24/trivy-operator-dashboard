using Microsoft.Extensions.Options;
using TrivyOperator.Dashboard.Application.CveEnrichment.Abstractions;
using TrivyOperator.Dashboard.Application.CveEnrichment.Models;
using TrivyOperator.Dashboard.Application.CveEnrichment.Options;
using TrivyOperator.Dashboard.Application.Shared.Cache.Abstractions;
using TrivyOperator.Dashboard.Domain.CveEnrichment.Entities;
using TrivyOperator.Dashboard.Domain.CveEnrichment.ValueObjects;
using TrivyOperator.Dashboard.Domain.Shared.ValueObjects;

namespace TrivyOperator.Dashboard.Application.CveEnrichment.Services;

// per CVE source, fetched in batches
// each tick (or on a rebuild request) fetches the subjects that are due:
// - never fetched (new CVEs first), or past the entry TTL (past the error TTL after a failure)
// - all of them on rebuild
// on a failed batch the tick stops (the source is likely down) and the batch CVEs are marked as Error,
// keeping their previous score; on a rate limit no request is sent until its Retry-After elapses
public sealed class EpssTimedHostedService(
    IEpssProvider provider,
    IOptionsMonitor<CveEnrichmentOptions> options,
    ICveEnrichmentRefreshTrigger trigger,
    ICache<CveId, Timestamp> subjects,
    ICache<CveId, CveInfo> infos,
    ILogger<EpssTimedHostedService> logger
) : BackgroundService
{
    private Timestamp pausedUntil = new(DateTime.MinValue);

    public override async Task StopAsync(CancellationToken ctx)
    {
        logger.LogInformation("CVE enrichment {source} Timed Hosted Service is stopping.", CveSources.Epss);
        await base.StopAsync(ctx);
        logger.LogInformation("CVE enrichment {source} Timed Hosted Service stopped.", CveSources.Epss);
    }

    protected override async Task ExecuteAsync(CancellationToken ctx)
    {
        logger.LogInformation("CVE enrichment {source} Timed Hosted Service started.", CveSources.Epss);

        // waits first - the startup is busy enough (watchers listing all the reports)
        while (!ctx.IsCancellationRequested)
        {
            bool rebuild;

            try
            {
                TimeSpan interval = TimeSpan.FromSeconds(Math.Max(1, options.CurrentValue.Epss.ProcessingIntervalInSeconds));
                rebuild = await trigger.WaitAsync(CveSources.Epss, interval, ctx);
            }
            catch (OperationCanceledException)
            {
                break;
            }

            try
            {
                await ProcessAsync(rebuild, ctx);
            }
            catch (OperationCanceledException) when (ctx.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                logger.LogError(
                    ex,
                    "Error occurred while processing CVE enrichment {source} - {exceptionMessage}",
                    CveSources.Epss,
                    ex.Message);
            }
        }
    }

    private async Task ProcessAsync(bool rebuild, CancellationToken ctx)
    {
        EpssOptions currentOptions = options.CurrentValue.Epss;
        Timestamp now = Timestamp.Now();

        if (rebuild)
        {
            logger.LogInformation("CVE enrichment {source} rebuild requested - all CVEs will be refetched.", CveSources.Epss);
        }
        else if (now < pausedUntil)
        {
            return;
        }

        TimeSpan entryTtl = TimeSpan.FromMinutes(Math.Max(1, currentOptions.EntryTtlInMinutes));
        TimeSpan errorTtl = TimeSpan.FromMinutes(Math.Max(1, currentOptions.ErrorTtlInMinutes));

        CveId[] due =
        [
            .. subjects.Keys
                .Select(id => (Id: id, Part: infos.TryGetValue(id, out CveInfo? info) ? info.Epss : null))
                .Where(x => rebuild || IsDue(x.Part, now, entryTtl, errorTtl))
                .OrderBy(static x => x.Part is null ? DateTime.MinValue : x.Part.FetchedAt.Value)
                .Select(static x => x.Id),
        ];

        if (due.Length == 0)
        {
            return;
        }

        int fetched = 0;

        foreach (CveId[] batch in due.Chunk(Math.Max(1, currentOptions.BatchSize)))
        {
            if (!await ProcessBatchAsync(batch, currentOptions.BaseUrl, errorTtl, ctx))
            {
                break;
            }

            fetched += batch.Length;
        }

        logger.LogDebug("CVE enrichment {source} - {fetched} of {due} due CVEs fetched.", CveSources.Epss, fetched, due.Length);
    }

    // returns false when the batch failed
    private async Task<bool> ProcessBatchAsync(CveId[] batch, string baseUrl, TimeSpan errorTtl, CancellationToken ctx)
    {
        CveProviderResult<EpssScore> result = await provider.GetScores(baseUrl, batch, ctx);
        Timestamp now = Timestamp.Now();

        if (result.Status == CveProviderStatuses.Ok && result.Items is not null)
        {
            Dictionary<CveId, EpssScore> scores = [];

            foreach (EpssScore score in result.Items)
            {
                scores[score.Id] = score;
            }

            foreach (CveId id in batch)
            {
                CveSourceResult<EpssScore> part = scores.TryGetValue(id, out EpssScore? score)
                    ? CveSourceResult<EpssScore>.Ok(score, now)
                    : CveSourceResult<EpssScore>.NotListed(now);

                infos.UpdatePart(subjects, id, info => info with { Epss = part, });
            }

            return true;
        }

        string errorMessage = result.ErrorMessage ?? $"Unexpected {result.Status} response";
        Timestamp nextTry = new(now.Value.Add(result.RetryAfter ?? errorTtl));

        if (result.RetryAfter is not null)
        {
            pausedUntil = nextTry;
        }

        logger.LogWarning(
            "Could not fetch CVE enrichment {source} scores - {errorMessage}. Next try at {nextTry}. " +
            "If the source is not reachable from the cluster, consider disabling it.",
            CveSources.Epss,
            errorMessage,
            nextTry);

        foreach (CveId id in batch)
        {
            infos.UpdatePart(
                subjects,
                id,
                info => info with { Epss = CveSourceResult<EpssScore>.Failed(info.Epss, errorMessage, now), });
        }

        return false;
    }

    private static bool IsDue(CveSourceResult<EpssScore>? part, Timestamp now, TimeSpan entryTtl, TimeSpan errorTtl)
    {
        if (part is null)
        {
            return true;
        }

        TimeSpan ttl = part.Status == CveSourceStatuses.Error ? errorTtl : entryTtl;

        return now.Value >= part.FetchedAt.Value.Add(ttl);
    }
}
