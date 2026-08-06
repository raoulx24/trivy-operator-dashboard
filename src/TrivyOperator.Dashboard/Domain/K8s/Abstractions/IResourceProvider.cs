namespace TrivyOperator.Dashboard.Domain.K8s.Abstractions;

public interface IResourceProvider<TResource>
{
    Task<IReadOnlyList<TResource>> GetResources(CancellationToken ctx = default);
}
