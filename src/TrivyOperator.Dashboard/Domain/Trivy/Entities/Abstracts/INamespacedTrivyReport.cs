using TrivyOperator.Dashboard.Domain.K8s.ValueObjects;

namespace TrivyOperator.Dashboard.Domain.Trivy.Entities.Abstracts;

public interface INamespacedTrivyReport<out TId> : ITrivyReport<TId>
{
    NamespaceName NamespaceName { get; }
}
