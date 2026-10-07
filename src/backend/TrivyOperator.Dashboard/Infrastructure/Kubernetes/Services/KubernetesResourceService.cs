using k8s;
using k8s.Models;
using TrivyOperator.Dashboard.Application.Kubernetes.Contexts.Abstractions;
using TrivyOperator.Dashboard.Domain.Kubernetes.ValueObjects;
using TrivyOperator.Dashboard.Infrastructure.Kubernetes.ClientFactory.Abstractions;
using TrivyOperator.Dashboard.Infrastructure.Kubernetes.Contexts.Abstractions;

namespace TrivyOperator.Dashboard.Infrastructure.Kubernetes.Services;

public abstract class KubernetesResourceService<TKubernetesObject>(
    IKubernetesClientFactory kubernetesClientFactory,
    IContextProvider contextProvider,
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
            currentContext = contextProvider.GetDefaultContext();
        }

        return currentContext;
    }

    public abstract Task<IList<TKubernetesObject>> GetResources(CancellationToken cancellationToken = default);
}
