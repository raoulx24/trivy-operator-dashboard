using TrivyOperator.Dashboard.Domain.K8s.Abstractions;
using TrivyOperator.Dashboard.Domain.Trivy.Entities.Abstracts;

namespace TrivyOperator.Dashboard.Domain.Trivy.Abstractions;

public interface INamespacedTrivyReportProvider<T> : INamespacedResourceProvider<T>
where T : INamespacedTrivyReport
{
    
}
