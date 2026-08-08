using k8s;
using TrivyOperator.Dashboard.Domain.K8s.ValueObjects;

namespace TrivyOperator.Dashboard.Infrastructure.K8s.ClientFactory.Abstractions;

public interface IKubernetesClientFactory
{
    // Here it should be IKubernetes but the interface does not expose all yet...
    Kubernetes GetClient(ContextName contextName);
    IEnumerable<ContextName> GetContexts();
    ContextName GetDefaultContext();
}
