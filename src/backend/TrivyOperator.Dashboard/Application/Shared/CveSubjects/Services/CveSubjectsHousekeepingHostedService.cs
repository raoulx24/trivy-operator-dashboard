using Microsoft.Extensions.Options;
using TrivyOperator.Dashboard.Application.Shared.Cache.Abstractions;
using TrivyOperator.Dashboard.Application.Shared.CveSubjects.Options;
using TrivyOperator.Dashboard.Domain.Shared.ValueObjects;

namespace TrivyOperator.Dashboard.Application.Shared.CveSubjects.Services;

// each tick, sweeps the CVEs not seen in any report scanned in the last SweepAgeInHours (when the sweep is enabled)
// a CVE refreshed by the collector right while it is swept is lost until the next event of its report (accepted race)
// the consumers drop their own data of the swept CVEs (e.g. CVE Enrichment housekeeping)
public sealed class CveSubjectsHousekeepingHostedService(
    IOptionsMonitor<CveSubjectsOptions> options,
    ICache<CveId, Timestamp> subjects,
    ILogger<CveSubjectsHousekeepingHostedService> logger
) : BackgroundService
{
    public override async Task StopAsync(CancellationToken ctx)
    {
        logger.LogInformation("CVE subjects Housekeeping Hosted Service is stopping.");
        await base.StopAsync(ctx);
        logger.LogInformation("CVE subjects Housekeeping Hosted Service stopped.");
    }

    protected override async Task ExecuteAsync(CancellationToken ctx)
    {
        logger.LogInformation("CVE subjects Housekeeping Hosted Service started.");

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
                    "Error occurred while CVE subjects housekeeping - {exceptionMessage}",
                    ex.Message);
            }
        }
    }

    private void Process()
    {
        int sweepAgeInHours = options.CurrentValue.SweepAgeInHours;

        if (sweepAgeInHours <= 0)
        {
            return;
        }

        Timestamp cutoff = new(DateTime.UtcNow.AddHours(-sweepAgeInHours));
        int swept = 0;

        foreach (KeyValuePair<CveId, Timestamp> subject in subjects.ToArray())
        {
            if (subject.Value < cutoff && subjects.TryRemove(subject.Key, out _))
            {
                swept++;
            }
        }

        if (swept > 0)
        {
            logger.LogDebug(
                "CVE subjects housekeeping - {swept} CVEs swept, {count} CVEs left.",
                swept,
                subjects.Count);
        }
    }
}
