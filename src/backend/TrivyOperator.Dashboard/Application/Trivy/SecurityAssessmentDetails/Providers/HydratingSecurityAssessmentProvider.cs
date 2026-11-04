using TrivyOperator.Dashboard.Application.Shared.Details.Providers;
using TrivyOperator.Dashboard.Domain.Kubernetes.ValueObjects;
using TrivyOperator.Dashboard.Domain.Shared.Details.Abstractions;
using TrivyOperator.Dashboard.Domain.Shared.Stores.Abstractions;
using TrivyOperator.Dashboard.Domain.Trivy.Entities.Abstracts;
using TrivyOperator.Dashboard.Domain.Trivy.ValueObjects.SecurityAssessments;
using TrivyOperator.Dashboard.Domain.Trivy.ValueObjects.Shared;

namespace TrivyOperator.Dashboard.Application.Trivy.SecurityAssessmentDetails.Providers;

// the security assessment reports with the checks from the checks store
// - a check without a data in the store (e.g. its event is not processed yet) becomes a placeholder
//   with the check id as title, so the report still shows all its rows
public sealed class HydratingSecurityAssessmentProvider<TReport, TSlim>(
    IResourceProvider<TSlim, Uid> slimProvider,
    IDetailProvider<Check, CheckId> checks,
    ILogger<HydratingSecurityAssessmentProvider<TReport, TSlim>> logger
) : HydratingResourceProvider<TReport, TSlim, Uid>(slimProvider)
    where TReport : class
    where TSlim : class, ISecurityAssessmentSlimReport<TSlim, TReport, Uid>
{
    protected override async Task<IReadOnlyList<TReport>> Hydrate(IReadOnlyList<TSlim> slims, CancellationToken ctx)
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

    protected override TReport ToSummary(TSlim slim) => slim.Hydrate([]);

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
