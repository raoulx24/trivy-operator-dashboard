using k8s;
using k8s.Models;
using System.Runtime.CompilerServices;
using System.Text.Json;
using TrivyOperator.Dashboard.Application.Kubernetes.ClientFactory.Abstractions;
using TrivyOperator.Dashboard.Domain.Kubernetes.ValueObjects;
using TrivyOperator.Dashboard.Infrastructure.Kubernetes.Contexts.Abstractions;
using TrivyOperator.Dashboard.Infrastructure.Kubernetes.CustomResources;
using TrivyOperator.Dashboard.Infrastructure.Kubernetes.Services.Abstractions;

namespace TrivyOperator.Dashboard.Infrastructure.Kubernetes.Services;

public class NamespacedCustomResourceService<TKubernetesObject>(
    IKubernetesClientFactory kubernetesClientFactory,
    IKubernetesContextResolver contextResolver,
    ICrdFactory customResourceDefinitionFactory,
    IClusterScopedResourceService<V1Namespace, V1NamespaceList> namespaceService
) : NamespacedResourceService<TKubernetesObject, CustomResourceList<TKubernetesObject>>(
    kubernetesClientFactory, contextResolver, namespaceService)

    where TKubernetesObject : CustomResource
{
    private CustomResourceDefinition Crd =>
        field ??= customResourceDefinitionFactory.Get<TKubernetesObject>();

    public override Task<CustomResourceList<TKubernetesObject>> GetResourceList(
        string namespaceName,
        int? pageLimit = null,
        string? continueToken = null,
        CancellationToken cancellationToken = default
    ) => GetKubernetesClient()
        .ListNamespacedCustomObjectAsync<CustomResourceList<TKubernetesObject>>(
            Crd.Group,
            Crd.Version,
            namespaceName,
            Crd.PluralName,
            limit: pageLimit,
            continueParameter: continueToken,
            cancellationToken: cancellationToken
        );

    public override Task<TKubernetesObject> GetResource(
        string resourceName,
        string namespaceName,
        CancellationToken cancellationToken = default
    ) => GetKubernetesClient()
        .CustomObjects.GetNamespacedCustomObjectAsync<TKubernetesObject>(
            Crd.Group,
            Crd.Version,
            namespaceName,
            Crd.PluralName,
            resourceName,
            cancellationToken
        );

    public override async IAsyncEnumerable<WatchEvent<TKubernetesObject>> GetResourceWatchList(
        string namespaceName,
        string? lastResourceVersion = null,
        int? timeoutSeconds = null,
        [EnumeratorCancellation] CancellationToken cancellationToken = default
    )
    {
        Exception? watchException = null;
        
        IAsyncEnumerable<(WatchEventType, object)> watchStream = GetKubernetesClient()
            .CustomObjects.WatchListNamespacedCustomObjectAsync(
                Crd.Group,
                Crd.Version,
                namespaceName,
                Crd.PluralName,
                resourceVersion: lastResourceVersion,
                allowWatchBookmarks: true,
                timeoutSeconds: timeoutSeconds,
                onError: ex => watchException = ex,
                cancellationToken: cancellationToken
            );
        await foreach ((WatchEventType type, object item) in watchStream)
        {
            yield return new WatchEvent<TKubernetesObject>
            {
                Type = type,
                Object = KubernetesJson.Deserialize<TKubernetesObject>(((JsonElement)item).GetRawText()),
            };
        }
        
        if (watchException is not null)
            throw watchException;
    }
}
