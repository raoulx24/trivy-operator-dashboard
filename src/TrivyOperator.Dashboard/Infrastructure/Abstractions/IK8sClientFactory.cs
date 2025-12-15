using k8s;

namespace TrivyOperator.Dashboard.Infrastructure.Abstractions;

public interface IKubernetesClientFactory
{
    Kubernetes GetClient(string contextName);
    public IEnumerable<string> GetContexts();
    public string GetCurrentContext();
}
