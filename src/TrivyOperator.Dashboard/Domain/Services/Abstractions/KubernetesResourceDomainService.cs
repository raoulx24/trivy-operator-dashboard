using k8s;
using k8s.Models;
using TrivyOperator.Dashboard.Infrastructure.Abstractions;

namespace TrivyOperator.Dashboard.Domain.Services.Abstractions;

public abstract class KubernetesResourceDomainService<TKubernetesObject>(
    IKubernetesClientFactory kubernetesClientFactory, IServiceScopeFactory scopeFactory)
    where TKubernetesObject : IKubernetesObject<V1ObjectMeta>, IMetadata<V1ObjectMeta>
{
    protected Kubernetes GetKubernetesClient()
    {
        using var scope = scopeFactory.CreateScope();
        var kubernetesContextProviderService = scope.ServiceProvider.GetRequiredService<IKubernetesContextProvider>();
        string currentContext = kubernetesContextProviderService.GetCurrentContext();

        return kubernetesClientFactory.GetClient(currentContext);
    }

    public abstract Task<IList<TKubernetesObject>> GetResources(CancellationToken? cancellationToken = null);
}
