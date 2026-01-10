using TrivyOperator.Dashboard.Application.K8s.Models;

namespace TrivyOperator.Dashboard.Application.K8s.Services.Contexts.Abstractions;

public interface IKubernetesContextService
{
    Task<IEnumerable<string>> GetContexts();
    Task<string> GetCurrentContext();
    Task<KubernetesContextsDto> GetKubernetesContextsDto();
}