using TrivyOperator.Dashboard.Application.Shared.Queries;
using TrivyOperator.Dashboard.Application.Shared.Queries.Services.Abstractions;

namespace TrivyOperator.Dashboard.Application.Kubernetes.Queries.Namespaces.Services.Abstractions;

public interface IKubernetesNamespaceService : IQueryService
{
    Task<IReadOnlyList<string>> GetKubernetesNamespaces(CancellationToken ctx = default);
}
