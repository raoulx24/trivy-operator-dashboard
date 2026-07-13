using k8s;

namespace TrivyOperator.Dashboard.Infrastructure.K8s.ClientFactory.Abstractions;

public interface IKubernetesClientFactory
{
    // Here it should be IKubernetes but the interface does not expose all yet...
    Kubernetes GetClient(string contextName);
    IEnumerable<string> GetContexts();
    string GetCurrentContext();
}
