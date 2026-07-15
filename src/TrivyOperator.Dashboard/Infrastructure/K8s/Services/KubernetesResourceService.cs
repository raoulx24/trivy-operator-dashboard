using k8s;
using k8s.Models;
using TrivyOperator.Dashboard.Infrastructure.K8s.ClientFactory.Abstractions;

namespace TrivyOperator.Dashboard.Infrastructure.K8s.Services;

public abstract class KubernetesResourceService<TKubernetesObject>(
    IKubernetesClientFactory kubernetesClientFactory,
    IServiceScopeFactory scopeFactory
)
    where TKubernetesObject : IKubernetesObject<V1ObjectMeta>, IMetadata<V1ObjectMeta>
{
    protected Kubernetes GetKubernetesClient()
    {
        using IServiceScope scope = scopeFactory.CreateScope();
        IKubernetesContextProvider kubernetesContextProviderService =
            scope.ServiceProvider.GetRequiredService<IKubernetesContextProvider>();
        if (!kubernetesContextProviderService.TryGetCurrentContext(out string? currentContext) ||
            string.IsNullOrWhiteSpace(currentContext))
        {
            currentContext = kubernetesClientFactory.GetCurrentContext();
        }

        return kubernetesClientFactory.GetClient(currentContext);
    }

    public abstract Task<IList<TKubernetesObject>> GetResources(CancellationToken cancellationToken = default);
}
