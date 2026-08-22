using TrivyOperator.Dashboard.Application.Queries.Namespaces.Services.Abstractions;
using TrivyOperator.Dashboard.Domain.K8s.ValueObjects;
using TrivyOperator.Dashboard.Domain.Shared.Stores.Abstractions;

namespace TrivyOperator.Dashboard.Application.Queries.Namespaces.Services;

public class KubernetesNamespaceService(IResourceProvider<NamespaceName, Uid> resourceProvider) : IKubernetesNamespaceService
{
    public async Task<IReadOnlyList<string>> GetKubernetesNamespaces(CancellationToken ctx = default)
    {
        IReadOnlyList<NamespaceName> namespaceNames = await resourceProvider.GetResources(ctx);
        

        return [.. namespaceNames.Select(x => x.Value),];
    }
}
