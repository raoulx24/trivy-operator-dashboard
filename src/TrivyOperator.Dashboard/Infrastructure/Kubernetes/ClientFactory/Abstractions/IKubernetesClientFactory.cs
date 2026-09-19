using TrivyOperator.Dashboard.Domain.Kubernetes.ValueObjects;

namespace TrivyOperator.Dashboard.Infrastructure.Kubernetes.ClientFactory.Abstractions;

public interface IKubernetesClientFactory
{
    // Here it should be IKubernetes but the interface does not expose all yet...
    k8s.Kubernetes GetClient(ContextName contextName);
}
