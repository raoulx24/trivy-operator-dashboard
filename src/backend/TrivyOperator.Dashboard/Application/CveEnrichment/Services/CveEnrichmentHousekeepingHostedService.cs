using Microsoft.Extensions.Options;
using TrivyOperator.Dashboard.Application.CveEnrichment.Options;
using TrivyOperator.Dashboard.Application.Shared.Cache.Abstractions;
using TrivyOperator.Dashboard.Domain.CveEnrichment.Entities;
using TrivyOperator.Dashboard.Domain.CveEnrichment.ValueObjects;
using TrivyOperator.Dashboard.Domain.Shared.ValueObjects;

namespace TrivyOperator.Dashboard.Application.CveEnrichment.Services;

// each tick:
// - sweeps the CVEs not seen in any report scanned in the last SweepAgeInHours (when the sweep is enabled)
// - drops the enrichment of CVEs that are no longer subjects
// a CVE refreshed by the collector right while it is swept is lost until the next event of its report (accepted race)
public sealed class CveEnrichmentHousekeepingHostedService(
    IOptionsMonitor<CveEnrichmentOptions> options,
    ICache<CveId, Timestamp> subjects,
    ICache<CveId, CveInfo> infos,
    ILogger<CveEnrichmentHousekeepingHostedService> logger
) : BackgroundService
{
    public override async Task StopAsync(CancellationToken ctx)
    {
        logger.LogInformation("CVE enrichment Housekeeping Hosted Service is stopping.");
        await base.StopAsync(ctx);
        logger.LogInformation("CVE enrichment Housekeeping Hosted Service stopped.");
    }

    protected override async Task ExecuteAsync(CancellationToken ctx)
    {
        logger.LogInformation("CVE enrichment Housekeeping Hosted Service started.");

        while (!ctx.IsCancellationRequested)
        {
            try
            {
                TimeSpan interval = TimeSpan.FromSeconds(Math.Max(1, options.CurrentValue.HousekeepingIntervalInSeconds));
                await Task.Delay(interval, ctx);
            }
            catch (OperationCanceledException)
            {
                break;
            }

            try
            {
                Process();
            }
            catch (Exception ex)
            {
                logger.LogError(
                    ex,
                    "Error occurred while CVE enrichment housekeeping - {exceptionMessage}",
                    ex.Message);
            }
        }
    }

    private void Process()
    {
        int sweepAgeInHours = options.CurrentValue.SweepAgeInHours;
        int swept = 0;

        if (sweepAgeInHours > 0)
        {
            Timestamp cutoff = new(DateTime.UtcNow.AddHours(-sweepAgeInHours));

            foreach (KeyValuePair<CveId, Timestamp> subject in subjects.ToArray())
            {
                if (subject.Value < cutoff && subjects.TryRemove(subject.Key, out _))
                {
                    swept++;
                }
            }
        }

        int dropped = 0;

        foreach (CveId id in infos.Keys.ToArray())
        {
            if (!subjects.ContainsKey(id) && infos.TryRemove(id, out _))
            {
                dropped++;
            }
        }

        if (swept > 0 || dropped > 0)
        {
            logger.LogDebug(
                "CVE enrichment housekeeping - {swept} CVEs swept, {dropped} enrichments dropped, {count} CVEs left.",
                swept,
                dropped,
                subjects.Count);
        }
    }
}
