namespace TrivyOperator.Dashboard.Application.K8s.Services.Namespaces.Abstractions;

public interface INamespaceService
{
    Task<IEnumerable<string>> GetKubernetesNamespaces();
}
