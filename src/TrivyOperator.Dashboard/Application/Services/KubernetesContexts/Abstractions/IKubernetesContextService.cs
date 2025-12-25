using TrivyOperator.Dashboard.Application.Models;

namespace TrivyOperator.Dashboard.Application.Services.KubernetesContexts.Abstractions;

public interface IKubernetesContextService
{
    Task<IEnumerable<string>> GetContexts();
    Task<string> GetCurrentContext();
    Task<KubernetesContextsDto> GetKubernetesContextsDto();
}