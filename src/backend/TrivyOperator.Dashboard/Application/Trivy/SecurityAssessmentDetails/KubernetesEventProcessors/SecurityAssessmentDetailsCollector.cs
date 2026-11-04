using TrivyOperator.Dashboard.Application.Shared.Details.KubernetesEventProcessors;
using TrivyOperator.Dashboard.Domain.Shared.Details.Abstractions;
using TrivyOperator.Dashboard.Domain.Trivy.Entities.Abstracts;
using TrivyOperator.Dashboard.Domain.Trivy.ValueObjects.SecurityAssessments;

namespace TrivyOperator.Dashboard.Application.Trivy.SecurityAssessmentDetails.KubernetesEventProcessors;

// feeds the checks store from the reports of the events
public sealed class SecurityAssessmentDetailsCollector<TReport, TKey>(
    IDetailStore<Check, CheckId> details,
    ILogger<SecurityAssessmentDetailsCollector<TReport, TKey>> logger
) : DetailsCollector<TReport, TKey>(logger)
    where TReport : class, ISecurityAssessmentReport<TReport, TKey>
{
    protected override async Task Collect(TReport report, CancellationToken ctx)
    {
        // a check failing for several containers is in the report several times
        HashSet<CheckId> ids = [];

        foreach (Check check in report.Checks)
        {
            if (ids.Add(check.CheckId))
            {
                await details.Upsert(check, report.LastSeenAt, ctx);
            }
        }
    }
}
