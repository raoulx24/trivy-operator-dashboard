using TrivyOperator.Dashboard.Domain.K8s.ValueObjects;

namespace TrivyOperator.Dashboard.Domain.K8s.Abstractions;

public interface IResourceProvider<TResource>
{
    Task<IReadOnlyList<TResource>> GetResources(ContextName contextName = default, CancellationToken ctx = default);
}
