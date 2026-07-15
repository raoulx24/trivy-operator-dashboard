using TrivyOperator.Dashboard.Domain.K8s.ValueObjects;

namespace TrivyOperator.Dashboard.Infrastructure.ResourceStore.Abstractions;

public interface INamespacedResourceStore<T> : IResourceStore<T>
{
    Task<IReadOnlyCollection<T>> GetResources(NamespaceName namespaceName, CancellationToken? ctx = null);
}
