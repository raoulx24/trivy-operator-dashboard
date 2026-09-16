using TrivyOperator.Dashboard.Application.KubernetesEventPipeline.Models;

namespace TrivyOperator.Dashboard.Application.Queries.Contexts.Abstractions;

public interface IKubernetesContextService
{
    Task<KubernetesContextsDto> GetKubernetesContextsDto(CancellationToken ctx = default);
}
