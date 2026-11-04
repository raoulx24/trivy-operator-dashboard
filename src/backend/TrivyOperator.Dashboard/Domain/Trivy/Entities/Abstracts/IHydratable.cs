namespace TrivyOperator.Dashboard.Domain.Trivy.Entities.Abstracts;

// a slim report that gets its details back and becomes the full report again
public interface IHydratable<out TReport, in TDetail>
{
    // an empty list gives the report without details (the "summary" form)
    TReport Hydrate(IReadOnlyList<TDetail> details);
}
