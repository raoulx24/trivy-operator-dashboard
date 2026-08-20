namespace TrivyOperator.Dashboard.Application.Queries.Namespaces.Services.Abstractions;

public interface IKubernetesNamespaceService
{
    Task<IReadOnlyList<string>> GetKubernetesNamespaces(CancellationToken ctx = default);
}
