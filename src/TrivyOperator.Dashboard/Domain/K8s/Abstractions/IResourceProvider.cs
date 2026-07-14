namespace TrivyOperator.Dashboard.Domain.K8s.Abstractions;

public interface IResourceProvider<T>
{
    Task<IReadOnlyList<T>> GetResources(CancellationToken? cancellationToken = null);
}
