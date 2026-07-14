using TrivyOperator.Dashboard.Domain.K8s.ValueObjects;

namespace TrivyOperator.Dashboard.Domain.K8s.Abstractions;

public interface INamespacedResourceProvider<T> : IResourceProvider<T>
{
    Task<IReadOnlyList<T>> GetResources(NamespaceName namespaceName, CancellationToken? cancellationToken = null);
}
