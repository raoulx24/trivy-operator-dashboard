using k8s;
using k8s.Models;
using System.Text.Json;
using TrivyOperator.Dashboard.Domain.K8s.ValueObjects;
using TrivyOperator.Dashboard.Domain.TrivyOld.CustomResources.Abstractions;
using TrivyOperator.Dashboard.Infrastructure.K8s.ClientFactory.Abstractions;
using TrivyOperator.Dashboard.Infrastructure.K8s.Services.Abstractions;

namespace TrivyOperator.Dashboard.Infrastructure.K8s.Services;

public class NamespacedCustomResourceService<TKubernetesObject>(
    IKubernetesClientFactory kubernetesClientFactory,
    IServiceScopeFactory scopeFactory,
    ICrdFactory customResourceDefinitionFactory,
    IClusterScopedResourceWatchService<V1Namespace, V1NamespaceList> namespaceService
) : NamespacedResourceService<TKubernetesObject, CustomResourceList<TKubernetesObject>>(
    kubernetesClientFactory,
    scopeFactory,
    namespaceService
)
    where TKubernetesObject : CustomResource
{
    protected CustomResourceDefinition Crd =>
        field ??= customResourceDefinitionFactory.Get<TKubernetesObject>();

    public override Task<CustomResourceList<TKubernetesObject>> GetResourceList(
        string namespaceName,
        int? pageLimit = null,
        string? continueToken = null,
        CancellationToken? cancellationToken = null
    ) => GetKubernetesClient()
        .ListNamespacedCustomObjectAsync<CustomResourceList<TKubernetesObject>>(
            Crd.Group,
            Crd.Version,
            namespaceName,
            Crd.PluralName,
            limit: pageLimit,
            continueParameter: continueToken,
            cancellationToken: cancellationToken ?? CancellationToken.None
        );

    public override Task<TKubernetesObject> GetResource(
        string resourceName,
        string namespaceName,
        CancellationToken? cancellationToken = null
    ) => GetKubernetesClient()
        .CustomObjects.GetNamespacedCustomObjectAsync<TKubernetesObject>(
            Crd.Group,
            Crd.Version,
            namespaceName,
            Crd.PluralName,
            resourceName,
            cancellationToken ?? CancellationToken.None
        );

    public override async IAsyncEnumerable<WatchEvent<TKubernetesObject>> GetResourceWatchList(
        string namespaceName,
        string? lastResourceVersion = null,
        int? timeoutSeconds = null,
        Action<Exception>? onError = null,
        CancellationToken? cancellationToken = null
    )
    {
        IAsyncEnumerable<(WatchEventType, object)> watchStream = GetKubernetesClient()
            .CustomObjects.WatchListNamespacedCustomObjectAsync(
                Crd.Group,
                Crd.Version,
                namespaceName,
                Crd.PluralName,
                resourceVersion: lastResourceVersion,
                allowWatchBookmarks: true,
                timeoutSeconds: timeoutSeconds,
                onError: onError,
                cancellationToken: cancellationToken ?? CancellationToken.None
            );
        await foreach ((WatchEventType type, object item) in watchStream)
        {
            yield return new WatchEvent<TKubernetesObject>
            {
                Type = type,
                Object = KubernetesJson.Deserialize<TKubernetesObject>(((JsonElement)item).GetRawText()),
            };
        }
    }
}
