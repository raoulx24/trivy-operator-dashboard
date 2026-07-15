using TrivyOperator.Dashboard.Domain.Trivy.Entities.Abstracts;

namespace TrivyOperator.Dashboard.Infrastructure.ResourceStore.Abstractions;

public interface IClusterScopedTrivyReportStore<T> : INamespacedResourceStore<T>
    where T : IClusterScopedTrivyReport
{
}
