using TrivyOperator.Dashboard.Application.K8s.Models;
using TrivyOperator.Dashboard.Application.K8s.Services.Contexts.Abstractions;
using TrivyOperator.Dashboard.Domain.K8s.ValueObjects;
using TrivyOperator.Dashboard.Infrastructure.K8s.ClientFactory.Abstractions;

namespace TrivyOperator.Dashboard.Application.K8s.Services.Contexts;

public class KubernetesContextService(IKubernetesClientFactory kubernetesClientFactory) : IKubernetesContextService
{
    public Task<IEnumerable<string>> GetContexts() => Task.FromResult(kubernetesClientFactory.GetContexts().Select(x => x.Value));

    public Task<string> GetCurrentContext() => Task.FromResult(kubernetesClientFactory.GetDefaultContext().Value);

    public Task<KubernetesContextsDto> GetKubernetesContextsDto()
    {
        KubernetesContextsDto contextDto = new()
        {
            Contexts = [.. kubernetesClientFactory.GetContexts().Select(x => x.Value),],
            Current = kubernetesClientFactory.GetDefaultContext().Value,
        };

        return Task.FromResult(contextDto);
    }
}
