using TrivyOperator.Dashboard.Application.K8s.Models;
using TrivyOperator.Dashboard.Application.K8s.Services.Contexts.Abstractions;
using TrivyOperator.Dashboard.Domain.K8s.UpstreamAbstractions;

namespace TrivyOperator.Dashboard.Application.K8s.Services.Contexts;

public class KubernetesContextService(IKubernetesClientFactory kubernetesClientFactory) : IKubernetesContextService
{
    public Task<IEnumerable<string>> GetContexts() => Task.FromResult(kubernetesClientFactory.GetContexts());

    public Task<string> GetCurrentContext() => Task.FromResult(kubernetesClientFactory.GetCurrentContext());

    public Task<KubernetesContextsDto> GetKubernetesContextsDto()
    {
        KubernetesContextsDto contextDto = new()
        {
            Contexts = [.. kubernetesClientFactory.GetContexts(),],
            Current = kubernetesClientFactory.GetCurrentContext(),
        };

        return Task.FromResult(contextDto);
    }
}
