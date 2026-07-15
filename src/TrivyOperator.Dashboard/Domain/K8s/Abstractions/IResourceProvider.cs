using TrivyOperator.Dashboard.Domain.K8s.ValueObjects;

namespace TrivyOperator.Dashboard.Domain.K8s.Abstractions;

public interface IResourceProvider<T>
{
    Task<IReadOnlyList<T>> GetResources(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<T>> GetResources(NamespaceName namespaceName = default, CancellationToken cancellationToken = default);
}
