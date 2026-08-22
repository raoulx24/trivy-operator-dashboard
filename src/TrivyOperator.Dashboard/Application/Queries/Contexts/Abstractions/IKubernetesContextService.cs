using TrivyOperator.Dashboard.Application.K8sEventPipeline.Models;

namespace TrivyOperator.Dashboard.Application.Queries.Contexts.Abstractions;

public interface IKubernetesContextService
{
    Task<KubernetesContextsDto> GetKubernetesContextsDto(CancellationToken ctx = default);
}
