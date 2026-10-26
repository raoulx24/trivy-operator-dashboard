using TrivyOperator.Dashboard.Application.CveEnrichment.Abstractions;
using TrivyOperator.Dashboard.Application.CveEnrichment.Models;
using TrivyOperator.Dashboard.Application.Shared.Cache.Abstractions;
using TrivyOperator.Dashboard.Domain.CveEnrichment.Entities;
using TrivyOperator.Dashboard.Domain.CveEnrichment.ValueObjects;
using TrivyOperator.Dashboard.Domain.Shared.ValueObjects;

namespace TrivyOperator.Dashboard.Application.CveEnrichment.Services;

// base for the bulk sources (KEV, EUVD) - the whole catalog is downloaded and kept in memory
// each tick (or on a rebuild request):
// - downloads the catalog when it is past its refresh interval (always on rebuild)
// - writes the source part of every subject from the in memory catalog, so new CVEs get their part
//   within a tick, without any request; unchanged parts are not rewritten
// a failed download is retried only after the error TTL (or the Retry-After of the source);
// the previous catalog is still used, with the parts marked as Error
public abstract class CveCatalogTimedHostedService<T>(
    ICveEnrichmentRefreshTrigger trigger,
    ICache<CveId, Timestamp> subjects,
    ICache<CveId, CveInfo> infos,
    ILogger logger
) : BackgroundService
    where T : class
{
    private CveCatalogFetch<T>? catalog;

    protected abstract CveSource Source { get; }
    protected abstract TimeSpan ProcessingInterval { get; }
    protected abstract TimeSpan RefreshInterval { get; }
    protected abstract TimeSpan ErrorTtl { get; }

    protected abstract Task<CveProviderResult<T>> FetchCatalog(string? validator, CancellationToken ctx);
    protected abstract CveId GetId(T entry);
    protected abstract CveSourceResult<T>? GetPart(CveInfo info);
    protected abstract CveInfo WithPart(CveInfo info, CveSourceResult<T> part);

    public override async Task StopAsync(CancellationToken ctx)
    {
        logger.LogInformation("CVE enrichment {source} Timed Hosted Service is stopping.", Source);
        await base.StopAsync(ctx);
        logger.LogInformation("CVE enrichment {source} Timed Hosted Service stopped.", Source);
    }

    protected override async Task ExecuteAsync(CancellationToken ctx)
    {
        logger.LogInformation("CVE enrichment {source} Timed Hosted Service started.", Source);

        // waits first - the startup is busy enough (watchers listing all the reports)
        while (!ctx.IsCancellationRequested)
        {
            bool rebuild;

            try
            {
                rebuild = await trigger.WaitAsync(Source, ProcessingInterval, ctx);
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
                    Source,
                    ex.Message);
            }
        }
    }

    private async Task ProcessAsync(bool rebuild, CancellationToken ctx)
    {
        if (rebuild)
        {
            logger.LogInformation("CVE enrichment {source} rebuild requested - the catalog will be refetched.", Source);
            catalog = catalog?.MarkStale();
        }

        if (catalog is null || catalog.IsDue(Timestamp.Now()))
        {
            catalog = await FetchAsync(catalog, ctx);
        }

        int changed = 0;

        foreach (CveId id in subjects.Keys.ToArray())
        {
            if (infos.UpdatePart(subjects, id, info => WithPart(info, Resolve(id, GetPart(info)))))
            {
                changed++;
            }
        }

        if (changed > 0)
        {
            logger.LogDebug("CVE enrichment {source} - {changed} CVEs updated.", Source, changed);
        }
    }

    private CveSourceResult<T> Resolve(CveId id, CveSourceResult<T>? previous)
    {
        CveCatalogFetch<T> current = catalog!;

        if (current.Entries is null)
        {
            return CveSourceResult<T>.Failed(previous, current.ErrorMessage ?? "Not fetched", current.FetchedAt);
        }

        CveSourceResult<T> resolved = current.Entries.TryGetValue(id, out T? entry)
            ? CveSourceResult<T>.Ok(entry, current.FetchedAt)
            : CveSourceResult<T>.NotListed(current.FetchedAt);

        // the catalog is the one from a previous successful download
        return current.Status == CveSourceStatuses.Error
            ? resolved with { Status = CveSourceStatuses.Error, ErrorMessage = current.ErrorMessage, }
            : resolved;
    }

    private async Task<CveCatalogFetch<T>> FetchAsync(CveCatalogFetch<T>? previous, CancellationToken ctx)
    {
        CveProviderResult<T> result = await FetchCatalog(previous?.Entries is null ? null : previous.Validator, ctx);
        Timestamp now = Timestamp.Now();

        if (result.Status == CveProviderStatuses.Ok && result.Items is not null)
        {
            Dictionary<CveId, T> entries = [];

            foreach (T item in result.Items)
            {
                entries[GetId(item)] = item;
            }

            logger.LogInformation("CVE enrichment {source} catalog fetched - {count} CVEs.", Source, entries.Count);

            return new CveCatalogFetch<T>(
                CveSourceStatuses.Ok,
                entries,
                result.Validator,
                null,
                now,
                Add(now, RefreshInterval));
        }

        if (result.Status == CveProviderStatuses.NotModified && previous?.Entries is not null)
        {
            logger.LogDebug("CVE enrichment {source} catalog not modified.", Source);

            return previous with
            {
                Status = CveSourceStatuses.Ok,
                ErrorMessage = null,
                FetchedAt = now,
                NextFetchAt = Add(now, RefreshInterval),
            };
        }

        // a NotModified without a cached catalog drops the validator, so the next try is a full fetch
        string errorMessage = result.ErrorMessage ?? $"Unexpected {result.Status} response";
        Timestamp nextFetchAt = Add(now, result.RetryAfter ?? ErrorTtl);

        logger.LogWarning(
            "Could not fetch CVE enrichment {source} catalog - {errorMessage}. Next try at {nextFetchAt}. " +
            "If the source is not reachable from the cluster, consider disabling it.",
            Source,
            errorMessage,
            nextFetchAt);

        return new CveCatalogFetch<T>(
            CveSourceStatuses.Error,
            previous?.Entries,
            previous?.Entries is null ? null : previous.Validator,
            errorMessage,
            now,
            nextFetchAt);
    }

    protected static TimeSpan FromSeconds(int seconds) => TimeSpan.FromSeconds(Math.Max(1, seconds));

    protected static TimeSpan FromMinutes(int minutes) => TimeSpan.FromMinutes(Math.Max(1, minutes));

    private static Timestamp Add(Timestamp timestamp, TimeSpan span) => new(timestamp.Value.Add(span));
}
