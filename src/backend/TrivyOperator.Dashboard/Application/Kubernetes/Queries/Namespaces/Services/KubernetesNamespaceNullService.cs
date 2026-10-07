using TrivyOperator.Dashboard.Application.Kubernetes.Queries.Namespaces.Services.Abstractions;

namespace TrivyOperator.Dashboard.Application.Kubernetes.Queries.Namespaces.Services;

public class KubernetesNamespaceNullService : IKubernetesNamespaceService
{
    public Task<IReadOnlyList<string>> GetKubernetesNamespaces(CancellationToken ctx = default) =>
        Task.FromResult<IReadOnlyList<string>>([]);
}
