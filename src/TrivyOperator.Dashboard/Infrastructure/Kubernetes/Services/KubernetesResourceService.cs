using k8s;
using k8s.Models;
using TrivyOperator.Dashboard.Application.Kubernetes.ClientFactory.Abstractions;
using TrivyOperator.Dashboard.Domain.Kubernetes.ValueObjects;
using TrivyOperator.Dashboard.Infrastructure.Kubernetes.Contexts.Abstractions;

namespace TrivyOperator.Dashboard.Infrastructure.Kubernetes.Services;

public abstract class KubernetesResourceService<TKubernetesObject>(
    IKubernetesClientFactory kubernetesClientFactory,
    IKubernetesContextResolver contextResolver
)
    where TKubernetesObject : IKubernetesObject<V1ObjectMeta>, IMetadata<V1ObjectMeta>
{
    protected k8s.Kubernetes GetKubernetesClient()
    {
        return kubernetesClientFactory.GetClient(GetCurrentContext());
    }

    public ContextName GetCurrentContext()
    {
        if (!contextResolver.TryGetCurrentContext(out ContextName currentContext))
        {
            currentContext = kubernetesClientFactory.GetDefaultContext();
        }

        return currentContext;
    }

    public abstract Task<IList<TKubernetesObject>> GetResources(CancellationToken cancellationToken = default);
}
