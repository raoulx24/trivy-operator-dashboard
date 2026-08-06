using k8s;
using k8s.Models;
using TrivyOperator.Dashboard.Domain.K8s.ValueObjects;
using TrivyOperator.Dashboard.Infrastructure.K8s.ClientFactory.Abstractions;

namespace TrivyOperator.Dashboard.Infrastructure.K8s.Services;

public abstract class KubernetesResourceService<TKubernetesObject>(
    IKubernetesClientFactory kubernetesClientFactory,
    IKubernetesContextResolver contextResolver
)
    where TKubernetesObject : IKubernetesObject<V1ObjectMeta>, IMetadata<V1ObjectMeta>
{
    protected Kubernetes GetKubernetesClient()
    {
        if (!contextResolver.TryResolveCurrentContext(out ContextName currentContext))
        {
            currentContext = kubernetesClientFactory.GetCurrentContext();
        }

        return kubernetesClientFactory.GetClient(currentContext);
    }

    public abstract Task<IList<TKubernetesObject>> GetResources(CancellationToken cancellationToken = default);
}
