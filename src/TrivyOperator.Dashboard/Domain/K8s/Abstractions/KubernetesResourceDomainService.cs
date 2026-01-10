using k8s;
using k8s.Models;
using TrivyOperator.Dashboard.Domain.K8s.UpstreamAbstractions;

namespace TrivyOperator.Dashboard.Domain.K8s.Abstractions;

public abstract class KubernetesResourceDomainService<TKubernetesObject>(
    IKubernetesClientFactory kubernetesClientFactory, IServiceScopeFactory scopeFactory)
    where TKubernetesObject : IKubernetesObject<V1ObjectMeta>, IMetadata<V1ObjectMeta>
{
    protected Kubernetes GetKubernetesClient()
    {
        using IServiceScope scope = scopeFactory.CreateScope();
        IKubernetesContextProvider kubernetesContextProviderService = scope.ServiceProvider.GetRequiredService<IKubernetesContextProvider>();
        if (!kubernetesContextProviderService.TryGetCurrentContext(out string? currentContext) || string.IsNullOrWhiteSpace(currentContext))
        {
            currentContext = kubernetesClientFactory.GetCurrentContext(); 
        }
    
        return kubernetesClientFactory.GetClient(currentContext);
    }

    public abstract Task<IList<TKubernetesObject>> GetResources(CancellationToken? cancellationToken = null);
}
