using TrivyOperator.Dashboard.Domain.Trivy.ValueObjects.SecurityAssessments;

namespace TrivyOperator.Dashboard.Domain.Trivy.Entities.Abstracts;

public interface ISecurityAssessmentReport<out TSelf, out TId>
    : ITrivyReport<TId>
{
    IReadOnlyList<Check> Checks { get; }
    TSelf WithChecks(IReadOnlyList<Check> checks);
}
