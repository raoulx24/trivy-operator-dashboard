using TrivyOperator.Dashboard.Domain.Kubernetes.ValueObjects;
using TrivyOperator.Dashboard.Domain.Shared.Stores.Abstractions;
using TrivyOperator.Dashboard.Domain.Trivy.Entities.Abstracts;
using TrivyOperator.Dashboard.Domain.Trivy.ValueObjects.SecurityAssessments;
using TrivyOperator.Dashboard.Domain.Trivy.ValueObjects.Shared;

namespace TrivyOperator.Dashboard.Application.Trivy.SecurityAssessmentDetails.Providers;

// gives the query services the full reports back: the stored slim reports plus the checks from the checks store
// - a check without a data in the store (e.g. its event is not processed yet) becomes a placeholder
//   with the check id as title, so the report still shows all its rows
// - the summaries are the reports without checks, as before
public sealed class HydratingSecurityAssessmentProvider<TReport, TSlim>(
    IResourceProvider<TSlim, Uid> slimProvider,
    IDetailProvider<Check, CheckId> checks,
    ILogger<HydratingSecurityAssessmentProvider<TReport, TSlim>> logger
) : IResourceProvider<TReport, Uid>
    where TReport : class
    where TSlim : class, ISecurityAssessmentSlimReport<TSlim, TReport, Uid>
{
    public async Task<TReport?> GetResource(Uid key, CancellationToken ctx = default)
    {
        TSlim? slim = await slimProvider.GetResource(key, ctx);

        if (slim is null)
        {
            return null;
        }

        IReadOnlyList<TReport> result = await Hydrate([slim,], ctx);

        return result[0];
    }

    public async Task<IReadOnlyList<TReport>> GetResources(CancellationToken ctx = default)
    {
        return await Hydrate(await slimProvider.GetResources(ctx), ctx);
    }

    public async Task<IReadOnlyList<TReport>> GetResources(IEnumerable<Uid> keys, CancellationToken ctx = default)
    {
        return await Hydrate(await slimProvider.GetResources(keys, ctx), ctx);
    }

    public async Task<TReport?> GetResourceSummary(Uid key, CancellationToken ctx = default)
    {
        TSlim? slim = await slimProvider.GetResourceSummary(key, ctx);

        return slim?.Hydrate([]);
    }

    public async Task<IReadOnlyList<TReport>> GetResourceSummaries(CancellationToken ctx = default)
    {
        IReadOnlyList<TSlim> slims = await slimProvider.GetResourceSummaries(ctx);

        return [.. slims.Select(static x => x.Hydrate([])),];
    }

    public Task<IReadOnlyList<Uid>> GetResourceIds(CancellationToken ctx = default)
    {
        return slimProvider.GetResourceIds(ctx);
    }

    private async Task<IReadOnlyList<TReport>> Hydrate(IReadOnlyList<TSlim> slims, CancellationToken ctx)
    {
        if (slims.Count == 0)
        {
            return [];
        }

        HashSet<CheckId> ids = [.. slims.SelectMany(static x => x.CheckIds),];
        IReadOnlyDictionary<CheckId, Check> found = await checks.GetDetails(ids, ctx);

        if (found.Count < ids.Count)
        {
            logger.LogDebug(
                "{missing} of {count} checks of {kubernetesObjectType} have no data yet",
                ids.Count - found.Count,
                ids.Count,
                typeof(TReport).Name);
        }

        List<TReport> result = new(slims.Count);

        foreach (TSlim slim in slims)
        {
            List<Check> reportChecks = new(slim.CheckIds.Count);

            foreach (CheckId id in slim.CheckIds)
            {
                reportChecks.Add(found.TryGetValue(id, out Check? check) ? check : ToPlaceholder(id));
            }

            result.Add(slim.Hydrate(reportChecks));
        }

        return result;
    }

    private static Check ToPlaceholder(CheckId id)
    {
        return new Check(
            new Category(),
            id,
            new Description(),
            new Remediation(),
            Severities.Unknown,
            new Title(id.Value));
    }
}
