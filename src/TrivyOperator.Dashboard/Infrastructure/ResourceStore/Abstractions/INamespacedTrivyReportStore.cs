using TrivyOperator.Dashboard.Domain.Trivy.Entities.Abstracts;

namespace TrivyOperator.Dashboard.Infrastructure.ResourceStore.Abstractions;

public interface INamespacedTrivyReportStore<T, TId> : INamespacedResourceStore<T>
    where T : INamespacedTrivyReport<TId>
{
}
