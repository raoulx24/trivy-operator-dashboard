using k8s;
using k8s.Models;
using TrivyOperator.Dashboard.Domain.K8s.ValueObjects;
using TrivyOperator.Dashboard.Infrastructure.K8s.ClientFactory.Abstractions;
using TrivyOperator.Dashboard.Infrastructure.K8s.Services.Abstractions;

namespace TrivyOperator.Dashboard.Infrastructure.K8s.Services;

public abstract class NamespacedResourceService<TKubernetesObject, TKubernetesObjectList>(
    IKubernetesClientFactory kubernetesClientFactory,
    IServiceScopeFactory scopeFactory,
    IClusterScopedResourceWatchService<V1Namespace, V1NamespaceList> namespaceService
) : KubernetesResourceService<TKubernetesObject>(kubernetesClientFactory, scopeFactory),
    INamespacedResourceWatchService<TKubernetesObject, TKubernetesObjectList>
    where TKubernetesObject : IKubernetesObject<V1ObjectMeta>, IMetadata<V1ObjectMeta>
    where TKubernetesObjectList : IKubernetesObject<V1ListMeta>, IItems<TKubernetesObject>
{
    public override async Task<IList<TKubernetesObject>> GetResources(CancellationToken cancellationToken = default)
    {
        IEnumerable<V1Namespace> v1Namespaces = await namespaceService.GetResources(cancellationToken);
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

    public async Task<IList<TKubernetesObject>> GetResources(
        string namespaceName,
        CancellationToken cancellationToken = default
    )
    {
        TKubernetesObjectList kubernetesObjectList =
            await GetResourceList(namespaceName, cancellationToken: cancellationToken);
        return kubernetesObjectList.Items;
    }

    public abstract Task<TKubernetesObjectList> GetResourceList(
        string namespaceName,
        int? pageLimit = null,
        string? continueToken = null,
        CancellationToken cancellationToken = default
    );

    public abstract Task<TKubernetesObject> GetResource(
        string resourceName,
        string namespaceName,
        CancellationToken cancellationToken = default
    );

    public abstract IAsyncEnumerable<WatchEvent<TKubernetesObject>> GetResourceWatchList(
        string namespaceName,
        string? lastResourceVersion = null,
        int? timeoutSeconds = null,
        Action<Exception>? onError = null,
        CancellationToken cancellationToken = default
    );
}
