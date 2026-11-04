using Microsoft.Extensions.Options;
using TrivyOperator.Dashboard.Application.Trivy.SecurityAssessmentDetails.Abstractions;
using TrivyOperator.Dashboard.Application.Trivy.SecurityAssessmentDetails.Options;
using TrivyOperator.Dashboard.Domain.Kubernetes.ValueObjects;
using TrivyOperator.Dashboard.Domain.Shared.Stores;
using TrivyOperator.Dashboard.Domain.Shared.Stores.Abstractions;
using TrivyOperator.Dashboard.Domain.Shared.ValueObjects;
using TrivyOperator.Dashboard.Domain.Trivy.ValueObjects.SecurityAssessments;

namespace TrivyOperator.Dashboard.Application.Trivy.SecurityAssessmentDetails.Services;

// one per family of reports sharing a checks store (e.g. config audit and cluster config audit)
// each tick removes, per context, the checks that are not referenced by any stored report of the family
// and were not touched in the last GraceInHours (0 disables the removal)
// - the entries are read before the references: a report event arriving in between touches its checks,
//   which makes the removal of that entry skip (it only removes the entry instance it read)
// - the references are checked, not only the age: a report that is not rescanned does not touch its checks
public sealed class SecurityAssessmentDetailsHousekeepingHostedService(
    string familyName,
    IOptionsMonitor<SecurityAssessmentDetailsOptions> options,
    IDetailRepository<Check, CheckId> details,
    IReadOnlyList<IDetailReferenceProvider<CheckId>> references,
    ILogger<SecurityAssessmentDetailsHousekeepingHostedService> logger
) : BackgroundService
{
    public override async Task StopAsync(CancellationToken ctx)
    {
        logger.LogInformation("{family} details Housekeeping Hosted Service is stopping.", familyName);
        await base.StopAsync(ctx);
        logger.LogInformation("{family} details Housekeeping Hosted Service stopped.", familyName);
    }

    protected override async Task ExecuteAsync(CancellationToken ctx)
    {
        logger.LogInformation("{family} details Housekeeping Hosted Service started.", familyName);

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
                    "Error occurred while {family} details housekeeping - {exceptionMessage}",
                    familyName,
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

        Timestamp cutoff = new(DateTime.UtcNow.AddHours(-graceInHours));

        IReadOnlyDictionary<ContextName, IReadOnlyDictionary<CheckId, DetailEntry<Check>>> entries =
            await details.GetEntries(ctx);

        Dictionary<ContextName, HashSet<CheckId>> referenced = [];

        foreach (IDetailReferenceProvider<CheckId> provider in references)
        {
            foreach ((ContextName contextName, IReadOnlySet<CheckId> ids) in await provider.GetReferencedIds(ctx))
            {
                if (!referenced.TryGetValue(contextName, out HashSet<CheckId>? contextReferenced))
                {
                    contextReferenced = [];
                    referenced[contextName] = contextReferenced;
                }

                contextReferenced.UnionWith(ids);
            }
        }

        int swept = 0;

        foreach ((ContextName contextName, IReadOnlyDictionary<CheckId, DetailEntry<Check>> contextEntries) in entries)
        {
            referenced.TryGetValue(contextName, out HashSet<CheckId>? contextReferenced);

            foreach ((CheckId id, DetailEntry<Check> entry) in contextEntries)
            {
                if (entry.TouchedAt >= cutoff || contextReferenced?.Contains(id) == true)
                {
                    continue;
                }

                if (await details.RemoveIf(contextName, id, entry, ctx))
                {
                    swept++;
                }
            }
        }

        if (swept > 0)
        {
            logger.LogDebug("{family} details housekeeping - {swept} checks removed.", familyName, swept);
        }
    }
}
