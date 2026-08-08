using k8s;
using k8s.Models;
using TrivyOperator.Dashboard.Domain.K8s.ValueObjects;
using TrivyOperator.Dashboard.Infrastructure.K8s.ClientFactory.Abstractions;
using TrivyOperator.Dashboard.Infrastructure.K8s.Contexts.Abstractions;

namespace TrivyOperator.Dashboard.Infrastructure.K8s.Services;

public abstract class KubernetesResourceService<TKubernetesObject>(
    IKubernetesClientFactory kubernetesClientFactory,
    IKubernetesContextAccessor contextAccessor
)
    where TKubernetesObject : IKubernetesObject<V1ObjectMeta>, IMetadata<V1ObjectMeta>
{
    protected Kubernetes GetKubernetesClient()
    {
        return kubernetesClientFactory.GetClient(GetCurrentContext());
    }

    public ContextName GetCurrentContext()
    {
        if (!contextAccessor.TryGetCurrent(out ContextName currentContext))
        {
            currentContext = kubernetesClientFactory.GetDefaultContext();
        }

        return currentContext;
    }

    public abstract Task<IList<TKubernetesObject>> GetResources(CancellationToken cancellationToken = default);
}
