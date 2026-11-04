using TrivyOperator.Dashboard.Domain.Trivy.ValueObjects.SecurityAssessments;

namespace TrivyOperator.Dashboard.Domain.Trivy.Entities.Abstracts;

// a slim report that gets its checks back and becomes the full report again
public interface IHydratable<out TReport>
{
    // an empty list gives the report without checks (the "summary" form)
    TReport Hydrate(IReadOnlyList<Check> checks);
}
