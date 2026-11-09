using Microsoft.Extensions.Options;
using TrivyOperator.Dashboard.Application.Shared.Details.Abstractions;
using TrivyOperator.Dashboard.Application.Shared.Details.Options;
using TrivyOperator.Dashboard.Domain.History.FixedVersionHistory.Entities;
using TrivyOperator.Dashboard.Domain.History.FixedVersionHistory.Stores.Abstractions;
using TrivyOperator.Dashboard.Domain.Kubernetes.ValueObjects;
using TrivyOperator.Dashboard.Domain.Trivy.ValueObjects.Vulnerabilities;

namespace TrivyOperator.Dashboard.Application.History.FixedVersionHistory.Housekeeping;

// each tick removes the pairs (vulnerability id, package) that no stored vulnerability report references
// (of any kind: namespaced and cluster) and that no event touched in the last GraceInHours (0 disables the removal);
// same options as the details housekeeping (DetailsOptions)
// - the states are read before the references: an event in between touches the pair, and the conditional
//   delete keeps it
// - the first tick is one interval after the start, so the initial events have touched the pairs in use
public sealed class FixedVersionHistoryHousekeepingHostedService(
    IOptionsMonitor<DetailsOptions> options,
    IFixedVersionHistoryStore store,
    IReadOnlyList<IDetailReferenceProvider<PackageVulnerabilityKey>> references,
    ILogger<FixedVersionHistoryHousekeepingHostedService> logger
) : BackgroundService
{
    public override async Task StopAsync(CancellationToken ctx)
    {
        logger.LogInformation("Fixed version history Housekeeping Hosted Service is stopping.");
        await base.StopAsync(ctx);
        logger.LogInformation("Fixed version history Housekeeping Hosted Service stopped.");
    }

    protected override async Task ExecuteAsync(CancellationToken ctx)
    {
        logger.LogInformation("Fixed version history Housekeeping Hosted Service started.");

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
                await Process(ctx);
            }
            catch (OperationCanceledException) when (ctx.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                logger.LogError(
                    ex,
                    "Error occurred while fixed version history housekeeping - {exceptionMessage}",
                    ex.Message);
            }
        }
    }

    private async Task Process(CancellationToken ctx)
    {
        int graceInHours = options.CurrentValue.GraceInHours;

        if (graceInHours <= 0)
        {
            return;
        }

        DateTime cutoff = DateTime.UtcNow.AddHours(-graceInHours);

        IReadOnlyList<FixedVersionState> states = await store.GetAllStatesAsync(ctx);

        // the history is global: a pair in use in any context is kept
        HashSet<PackageVulnerabilityKey> referenced = [];

        foreach (IDetailReferenceProvider<PackageVulnerabilityKey> provider in references)
        {
            foreach ((ContextName _, IReadOnlySet<PackageVulnerabilityKey> ids) in await provider.GetReferencedIds(ctx))
            {
                referenced.UnionWith(ids);
            }
        }

        int swept = 0;

        foreach (FixedVersionState state in states)
        {
            ctx.ThrowIfCancellationRequested();

            if (state.TouchedAt.Value >= cutoff || referenced.Contains(state.Key))
            {
                continue;
            }

            if (await store.DeleteIfUntouchedAsync(state, ctx))
            {
                swept++;
            }
        }

        if (swept > 0)
        {
            logger.LogDebug("Fixed version history housekeeping - {swept} pairs removed.", swept);
        }
    }
}
