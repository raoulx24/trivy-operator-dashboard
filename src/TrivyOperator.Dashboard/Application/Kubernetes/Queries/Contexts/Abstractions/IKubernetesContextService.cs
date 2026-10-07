using TrivyOperator.Dashboard.Application.Kubernetes.Models;

namespace TrivyOperator.Dashboard.Application.Kubernetes.Queries.Contexts.Abstractions;

public interface IKubernetesContextService
{
    Task<KubernetesContextsDto> GetKubernetesContextsDto(CancellationToken ctx = default);
}
