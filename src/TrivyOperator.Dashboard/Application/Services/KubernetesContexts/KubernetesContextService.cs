using TrivyOperator.Dashboard.Application.Models;
using TrivyOperator.Dashboard.Application.Services.KubernetesContexts.Abstractions;
using TrivyOperator.Dashboard.Infrastructure.Abstractions;

namespace TrivyOperator.Dashboard.Application.Services.KubernetesContexts;

public class KubernetesContextService(IKubernetesClientFactory kubernetesClientFactory) : IKubernetesContextService
{
    public Task<IEnumerable<string>> GetContexts()
    {
        return Task.FromResult(kubernetesClientFactory.GetContexts());
    }

    public Task<string> GetCurrentContext()
    {
        return Task.FromResult(kubernetesClientFactory.GetCurrentContext());
    }

    public Task<KubernetesContextsDto> GetKubernetesContextsDto()
    {
        KubernetesContextsDto contextDto = new()
        {
            Contexts = [.. kubernetesClientFactory.GetContexts()],
            Current = kubernetesClientFactory.GetCurrentContext(),
        };

        return Task.FromResult(contextDto);
    }
}
