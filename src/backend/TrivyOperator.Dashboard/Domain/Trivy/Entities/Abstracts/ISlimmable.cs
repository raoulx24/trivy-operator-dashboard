namespace TrivyOperator.Dashboard.Domain.Trivy.Entities.Abstracts;

// a report that has a stored (slim) form
public interface ISlimmable<out TSlim>
{
    TSlim ToSlim();
}
