using TrivyOperator.Dashboard.Domain.Trivy.ValueObjects.SecurityAssessments;
using TrivyOperator.Dashboard.Domain.Trivy.ValueObjects.Shared;

namespace TrivyOperator.Dashboard.Domain.Trivy.Entities.Abstracts;

public interface ISecurityAssessmentReport<TSelf, out TId>
    : ITrivyReport<TSelf, TId>
where TSelf : ISecurityAssessmentReport<TSelf, TId>
{
    Scanner Scanner { get; }
    SeverityCounters SeverityCounters { get; }
    IReadOnlyList<Check> Checks { get; }
    TSelf WithChecks(IReadOnlyList<Check> checks);
}
