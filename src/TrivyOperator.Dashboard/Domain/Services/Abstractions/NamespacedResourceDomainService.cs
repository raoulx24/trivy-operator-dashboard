using k8s;
using k8s.Autorest;
using k8s.Models;
using TrivyOperator.Dashboard.Infrastructure.Abstractions;

namespace TrivyOperator.Dashboard.Domain.Services.Abstractions;

public abstract class NamespacedResourceDomainService<TKubernetesObject, TKubernetesObjectList>(
    IKubernetesClientFactory kubernetesClientFactory,
    IServiceScopeFactory scopeFactory,
    IClusterScopedResourceQueryDomainService<V1Namespace, V1NamespaceList> namespaceDomainService)
    : KubernetesResourceDomainService<TKubernetesObject>(kubernetesClientFactory, scopeFactory),
        INamespacedResourceWatchDomainService<TKubernetesObject, TKubernetesObjectList>
    where TKubernetesObject : IKubernetesObject<V1ObjectMeta>, IMetadata<V1ObjectMeta>
    where TKubernetesObjectList : IKubernetesObject<V1ListMeta>, IItems<TKubernetesObject>
{
    public override async Task<IList<TKubernetesObject>> GetResources(CancellationToken? cancellationToken = null)
    {
        IEnumerable<V1Namespace> v1Namespaces = await namespaceDomainService.GetResources(cancellationToken);
        List<TKubernetesObject> trivyReports = [];
        foreach (V1Namespace v1Namespace in v1Namespaces)
        {
            IList<TKubernetesObject> trivyReportsInNamespace =
                await GetResources(v1Namespace.Name(), cancellationToken);
            if (cancellationToken is { IsCancellationRequested: true, })
            {
                return [];
            }

            trivyReports.AddRange(trivyReportsInNamespace);
        }

        return trivyReports;
    }

    public async Task<IList<TKubernetesObject>> GetResources(string namespaceName, CancellationToken? cancellationToken = null)
    {
        TKubernetesObjectList kubernetesObjectList = await GetResourceList(namespaceName, cancellationToken: cancellationToken);
        return kubernetesObjectList.Items;
    }

    public abstract Task<TKubernetesObjectList> GetResourceList(
        string namespaceName,
        int? pageLimit = null,
        string? continueToken = null,
        CancellationToken? cancellationToken = null);

    public abstract Task<TKubernetesObject> GetResource(
        string resourceName,
        string namespaceName,
        CancellationToken? cancellationToken = null);

    public abstract Task<HttpOperationResponse<TKubernetesObjectList>> GetResourceWatchList(
        string namespaceName,
        string? lastResourceVersion = null,
        int? timeoutSeconds = null,
        CancellationToken? cancellationToken = null);
}
