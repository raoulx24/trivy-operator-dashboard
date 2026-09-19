using k8s;
using System.Runtime.CompilerServices;
using System.Text.Json;
using TrivyOperator.Dashboard.Application.Kubernetes.Contexts.Abstractions;
using TrivyOperator.Dashboard.Domain.Kubernetes.ValueObjects;
using TrivyOperator.Dashboard.Infrastructure.Kubernetes.ClientFactory.Abstractions;
using TrivyOperator.Dashboard.Infrastructure.Kubernetes.Contexts.Abstractions;
using TrivyOperator.Dashboard.Infrastructure.Kubernetes.CustomResources;
using TrivyOperator.Dashboard.Infrastructure.Kubernetes.Services.Abstractions;

namespace TrivyOperator.Dashboard.Infrastructure.Kubernetes.Services;

public class ClusterScopedCustomResourceService<TKubernetesObject>(
    IKubernetesClientFactory kubernetesClientFactory,
    IContextProvider contextProvider,
    IKubernetesContextResolver contextResolver,
    ICrdFactory customResourceDefinitionFactory
) : ClusterScopedResourceService<TKubernetesObject, CustomResourceList<TKubernetesObject>>(
    kubernetesClientFactory, contextProvider, contextResolver)
    where TKubernetesObject : CustomResource
{
    private CustomResourceDefinition Crd =>
        field ??= customResourceDefinitionFactory.Get<TKubernetesObject>();

    public override Task<CustomResourceList<TKubernetesObject>> GetResourceList(
        int? pageLimit = null,
        string? continueToken = null,
        CancellationToken cancellationToken = default
    ) => GetKubernetesClient()
        .ListClusterCustomObjectAsync<CustomResourceList<TKubernetesObject>>(
            Crd.Group,
            Crd.Version,
            Crd.PluralName,
            limit: pageLimit,
            continueParameter: continueToken,
            cancellationToken: cancellationToken
        );

    public override Task<TKubernetesObject>
        GetResource(string resourceName, CancellationToken cancellationToken = default) => GetKubernetesClient()
        .CustomObjects.GetClusterCustomObjectAsync<TKubernetesObject>(
            Crd.Group,
            Crd.Version,
            Crd.PluralName,
            resourceName,
            cancellationToken
        );

    public override async IAsyncEnumerable<WatchEvent<TKubernetesObject>> GetResourceWatchList(
        string? lastResourceVersion = null,
        int? timeoutSeconds = null,
        [EnumeratorCancellation] CancellationToken cancellationToken = default
    )
    {
        Exception? watchException = null;
        
        IAsyncEnumerable<(WatchEventType, object)> watchStream = GetKubernetesClient()
            .CustomObjects.WatchListClusterCustomObjectAsync(
                Crd.Group,
                Crd.Version,
                Crd.PluralName,
                resourceVersion: lastResourceVersion,
                allowWatchBookmarks: true,
                timeoutSeconds: timeoutSeconds,
                onError: ex => watchException = ex,
                cancellationToken: cancellationToken);
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
