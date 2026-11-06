using TrivyOperator.Dashboard.Application.Kubernetes.Queries.Namespaces.Services.Abstractions;
using TrivyOperator.Dashboard.Domain.Kubernetes.Entities;
using TrivyOperator.Dashboard.Domain.Kubernetes.ValueObjects;
using TrivyOperator.Dashboard.Domain.Shared.Stores.Abstractions;
using TrivyOperator.Dashboard.Application.Shared.Queries;
using TrivyOperator.Dashboard.Application.Shared.Queries.Services;

namespace TrivyOperator.Dashboard.Application.Kubernetes.Queries.Namespaces.Services;

public class KubernetesNamespaceService(IResourceProvider<KubernetesNamespace, Uid> resourceProvider) : QueryService, IKubernetesNamespaceService
{
    public async Task<IReadOnlyList<string>> GetKubernetesNamespaces(CancellationToken ctx = default)
    {
        IReadOnlyList<KubernetesNamespace> namespaceNames = await resourceProvider.GetResources(ctx);
        

        return [.. namespaceNames.Select(x => x.Name.Value),];
    }
}
