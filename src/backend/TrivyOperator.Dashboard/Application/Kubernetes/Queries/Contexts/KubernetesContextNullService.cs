using TrivyOperator.Dashboard.Application.Kubernetes.Models;
using TrivyOperator.Dashboard.Application.Kubernetes.Queries.Contexts.Abstractions;
using TrivyOperator.Dashboard.Application.Shared.Queries;
using TrivyOperator.Dashboard.Application.Shared.Queries.Services;

namespace TrivyOperator.Dashboard.Application.Kubernetes.Queries.Contexts;

// no Kubernetes services (e.g. the file repository) - no contexts
public class KubernetesContextNullService : QueryNullService, IKubernetesContextService
{
    public Task<KubernetesContextsDto> GetKubernetesContextsDto(CancellationToken ctx = default) =>
        Task.FromResult(new KubernetesContextsDto());
}
