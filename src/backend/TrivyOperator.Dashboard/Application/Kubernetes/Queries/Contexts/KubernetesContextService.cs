using TrivyOperator.Dashboard.Application.Kubernetes.Contexts.Abstractions;
using TrivyOperator.Dashboard.Application.Kubernetes.Models;
using TrivyOperator.Dashboard.Application.Kubernetes.Queries.Contexts.Abstractions;
using TrivyOperator.Dashboard.Application.Shared.Queries;
using TrivyOperator.Dashboard.Application.Shared.Queries.Services;

namespace TrivyOperator.Dashboard.Application.Kubernetes.Queries.Contexts;

public class KubernetesContextService(IContextProvider contextProvider) : QueryService, IKubernetesContextService
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
