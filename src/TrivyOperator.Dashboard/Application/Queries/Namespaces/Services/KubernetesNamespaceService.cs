using TrivyOperator.Dashboard.Application.Queries.Namespaces.Services.Abstractions;
using TrivyOperator.Dashboard.Domain.Kubernetes.Entities;
using TrivyOperator.Dashboard.Domain.Kubernetes.ValueObjects;
using TrivyOperator.Dashboard.Domain.Shared.Stores.Abstractions;

namespace TrivyOperator.Dashboard.Application.Queries.Namespaces.Services;

public class KubernetesNamespaceService(IResourceProvider<KubernetesNamespace, Uid> resourceProvider) : IKubernetesNamespaceService
{
    public async Task<IReadOnlyList<string>> GetKubernetesNamespaces(CancellationToken ctx = default)
    {
        IReadOnlyList<KubernetesNamespace> namespaceNames = await resourceProvider.GetResources(ctx);
        

        return [.. namespaceNames.Select(x => x.Name.Value),];
    }
}
