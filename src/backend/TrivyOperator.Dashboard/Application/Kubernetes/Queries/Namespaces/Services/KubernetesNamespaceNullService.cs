using TrivyOperator.Dashboard.Application.Kubernetes.Queries.Namespaces.Services.Abstractions;

using TrivyOperator.Dashboard.Application.Shared.Queries;
using TrivyOperator.Dashboard.Application.Shared.Queries.Services;

namespace TrivyOperator.Dashboard.Application.Kubernetes.Queries.Namespaces.Services;

public class KubernetesNamespaceNullService : QueryNullService, IKubernetesNamespaceService
{
    public Task<IReadOnlyList<string>> GetKubernetesNamespaces(CancellationToken ctx = default) =>
        Task.FromResult<IReadOnlyList<string>>([]);
}
