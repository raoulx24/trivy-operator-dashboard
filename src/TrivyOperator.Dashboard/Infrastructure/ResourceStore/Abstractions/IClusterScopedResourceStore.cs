using TrivyOperator.Dashboard.Domain.Trivy.Entities.Abstracts;

namespace TrivyOperator.Dashboard.Infrastructure.ResourceStore.Abstractions;

public interface IClusterScopedResourceStore<T> : IResourceStore<T>
{
}
