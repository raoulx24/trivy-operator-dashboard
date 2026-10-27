using Microsoft.Extensions.Options;
using TrivyOperator.Dashboard.Application.CveEnrichment.Options;
using TrivyOperator.Dashboard.Application.Shared.Cache.Abstractions;
using TrivyOperator.Dashboard.Domain.CveEnrichment.Entities;
using TrivyOperator.Dashboard.Domain.Shared.ValueObjects;

namespace TrivyOperator.Dashboard.Application.CveEnrichment.Services;

// each tick, drops the enrichment of CVEs that are no longer subjects (swept by the CVE subjects housekeeping)
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
        int dropped = 0;

        foreach (CveId id in infos.Keys.ToArray())
        {
            if (!subjects.ContainsKey(id) && infos.TryRemove(id, out _))
            {
                dropped++;
            }
        }

        if (dropped > 0)
        {
            logger.LogDebug(
                "CVE enrichment housekeeping - {dropped} enrichments dropped, {count} left.",
                dropped,
                infos.Count);
        }
    }
}
