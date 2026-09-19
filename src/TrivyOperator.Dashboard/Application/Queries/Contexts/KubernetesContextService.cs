using TrivyOperator.Dashboard.Application.Kubernetes.ClientFactory.Abstractions;
using TrivyOperator.Dashboard.Application.Kubernetes.Models;
using TrivyOperator.Dashboard.Application.Queries.Contexts.Abstractions;

namespace TrivyOperator.Dashboard.Application.Queries.Contexts;

public class KubernetesContextService(IKubernetesClientFactory kubernetesClientFactory) : IKubernetesContextService
{
    public Task<KubernetesContextsDto> GetKubernetesContextsDto(CancellationToken ctx = default)
    {
        ctx.ThrowIfCancellationRequested();
        
        KubernetesContextsDto contextDto = new()
        {
            Contexts = [.. kubernetesClientFactory.GetContexts().Select(x => x.Value),],
            Current = kubernetesClientFactory.GetDefaultContext().Value,
        };

        return Task.FromResult(contextDto);
    }
}
