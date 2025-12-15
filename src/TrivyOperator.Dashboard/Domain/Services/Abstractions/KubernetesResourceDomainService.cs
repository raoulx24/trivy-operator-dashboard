using k8s;
using k8s.Models;
using TrivyOperator.Dashboard.Infrastructure.Abstractions;

namespace TrivyOperator.Dashboard.Domain.Services.Abstractions;

public abstract class KubernetesResourceDomainService<TKubernetesObject>(
    IKubernetesClientFactory kubernetesClientFactory, IKubernetesContextProvider kubernetesContextProvider)
    where TKubernetesObject : IKubernetesObject<V1ObjectMeta>, IMetadata<V1ObjectMeta>
{
    protected readonly Kubernetes kubernetesClient = kubernetesClientFactory.GetClient(kubernetesContextProvider.GetCurrentContext());

    public abstract Task<IList<TKubernetesObject>> GetResources(CancellationToken? cancellationToken = null);
}
