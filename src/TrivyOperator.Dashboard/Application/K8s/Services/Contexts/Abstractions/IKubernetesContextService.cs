using TrivyOperator.Dashboard.Application.K8s.Models;
using TrivyOperator.Dashboard.Domain.K8s.ValueObjects;

namespace TrivyOperator.Dashboard.Application.K8s.Services.Contexts.Abstractions;

public interface IKubernetesContextService
{
    Task<KubernetesContextsDto> GetKubernetesContextsDto(CancellationToken ctx = default);
}
