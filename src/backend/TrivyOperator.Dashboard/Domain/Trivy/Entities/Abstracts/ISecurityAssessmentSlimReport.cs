using TrivyOperator.Dashboard.Domain.Trivy.ValueObjects.SecurityAssessments;
using TrivyOperator.Dashboard.Domain.Trivy.ValueObjects.Shared;

namespace TrivyOperator.Dashboard.Domain.Trivy.Entities.Abstracts;

public interface ISecurityAssessmentSlimReport<TSelf, out TReport, out TId>
    : ITrivyReport<TSelf, TId>, IHydratable<TReport>
where TSelf : ISecurityAssessmentSlimReport<TSelf, TReport, TId>
{
    Scanner Scanner { get; }
    SeverityCounters SeverityCounters { get; }

    // the ids of the checks, in report order; a check failing several times appears several times
    IReadOnlyList<CheckId> CheckIds { get; }
}
