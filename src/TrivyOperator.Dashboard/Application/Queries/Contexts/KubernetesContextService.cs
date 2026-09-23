using TrivyOperator.Dashboard.Application.Kubernetes.Contexts.Abstractions;
using TrivyOperator.Dashboard.Application.Kubernetes.Models;
using TrivyOperator.Dashboard.Application.Queries.Contexts.Abstractions;
using TrivyOperator.Dashboard.Application.Shared.Models;

namespace TrivyOperator.Dashboard.Application.Queries.Contexts;

public class KubernetesContextService(IContextProvider contextProvider) : IKubernetesContextService
{
    public Task<KubernetesContextsDto> GetKubernetesContextsDto(CancellationToken ctx = default)
    {
        ctx.ThrowIfCancellationRequested();
        
        KubernetesContextsDto contextDto = new()
        {
            Contexts = [.. contextProvider.GetContexts().Select(x => x.Value),],
            Current = contextProvider.GetDefaultContext().Value,
        };

        return Task.FromResult(contextDto);
    }
}
