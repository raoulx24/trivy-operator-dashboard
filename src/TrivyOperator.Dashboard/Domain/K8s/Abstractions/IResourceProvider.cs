using TrivyOperator.Dashboard.Domain.K8s.ValueObjects;

namespace TrivyOperator.Dashboard.Domain.K8s.Abstractions;

public interface IResourceProvider<TResource>
{
    Task<IReadOnlyList<TResource>> GetResources(CancellationToken ctx = default);
    Task<IReadOnlyList<TResource>> GetResources(NamespaceName namespaceName = default, CancellationToken ctx = default);
}
