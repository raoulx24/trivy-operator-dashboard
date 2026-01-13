namespace TrivyOperator.Dashboard.Application.K8s.Services.Namespaces.Abstractions;

public interface IKubernetesNamespaceService
{
    Task<IEnumerable<string>> GetKubernetesNamespaces();
}
