using TrivyOperator.Dashboard.Application.Kubernetes.Models;

using TrivyOperator.Dashboard.Application.Shared.Queries;
using TrivyOperator.Dashboard.Application.Shared.Queries.Services.Abstractions;

namespace TrivyOperator.Dashboard.Application.Kubernetes.Queries.Contexts.Abstractions;

public interface IKubernetesContextService : IQueryService
{
    Task<KubernetesContextsDto> GetKubernetesContextsDto(CancellationToken ctx = default);
}
