using TrivyOperator.Dashboard.Application.Queries.Namespaces.Services.Abstractions;

namespace TrivyOperator.Dashboard.Application.Queries.Namespaces.Services;

public class KubernetesNamespaceNullService : IKubernetesNamespaceService
{
    public Task<IReadOnlyList<string>> GetKubernetesNamespaces(CancellationToken ctx = default) =>
        Task.FromResult<IReadOnlyList<string>>([]);
}
