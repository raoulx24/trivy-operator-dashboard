using TrivyOperator.Dashboard.Application.K8sEventPipeline.Models;
using TrivyOperator.Dashboard.Application.Queries.Contexts.Abstractions;
using TrivyOperator.Dashboard.Infrastructure.K8s.ClientFactory.Abstractions;

namespace TrivyOperator.Dashboard.Application.Queries.Contexts;

public class KubernetesContextService(IKubernetesClientFactory kubernetesClientFactory) : IKubernetesContextService
{
    public Task<KubernetesContextsDto> GetKubernetesContextsDto(CancellationToken ctx = default)
    {
        KubernetesContextsDto contextDto = new()
        {
            Contexts = [.. kubernetesClientFactory.GetContexts().Select(x => x.Value),],
            Current = kubernetesClientFactory.GetDefaultContext().Value,
        };

        return Task.FromResult(contextDto);
    }
}
