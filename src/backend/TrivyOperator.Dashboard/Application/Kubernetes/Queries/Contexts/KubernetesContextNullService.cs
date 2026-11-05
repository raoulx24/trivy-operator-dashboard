using TrivyOperator.Dashboard.Application.Kubernetes.Models;
using TrivyOperator.Dashboard.Application.Kubernetes.Queries.Contexts.Abstractions;

namespace TrivyOperator.Dashboard.Application.Kubernetes.Queries.Contexts;

// no Kubernetes services (e.g. the file repository) - no contexts
public class KubernetesContextNullService : IKubernetesContextService
{
    public Task<KubernetesContextsDto> GetKubernetesContextsDto(CancellationToken ctx = default) =>
        Task.FromResult(new KubernetesContextsDto());
}
