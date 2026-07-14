using TrivyOperator.Dashboard.Domain.K8s.ValueObjects;

namespace TrivyOperator.Dashboard.Domain.Trivy.Entities.Abstracts;

public interface INamespacedTrivyReport : ITrivyReport
{
    NamespaceName NamespaceName { get; }
}
