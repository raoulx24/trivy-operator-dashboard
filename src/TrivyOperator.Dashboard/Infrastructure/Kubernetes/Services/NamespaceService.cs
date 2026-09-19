using k8s;
using k8s.Models;
using System.Runtime.CompilerServices;
using TrivyOperator.Dashboard.Application.Kubernetes.Contexts.Abstractions;
using TrivyOperator.Dashboard.Domain.Kubernetes.ValueObjects;
using TrivyOperator.Dashboard.Infrastructure.Kubernetes.ClientFactory.Abstractions;
using TrivyOperator.Dashboard.Infrastructure.Kubernetes.Contexts.Abstractions;

namespace TrivyOperator.Dashboard.Infrastructure.Kubernetes.Services;

public class NamespaceService(
    IKubernetesClientFactory kubernetesClientFactory,
    IContextProvider contextProvider,
    IKubernetesContextResolver contextResolver)
    : ClusterScopedResourceService<V1Namespace, V1NamespaceList>(kubernetesClientFactory, contextProvider, contextResolver)
{
    public override Task<V1Namespace> GetResource(string resourceName, CancellationToken cancellationToken = default) =>
        GetKubernetesClient()
            .CoreV1.ReadNamespaceAsync(resourceName, cancellationToken: cancellationToken);

    public override Task<V1NamespaceList> GetResourceList(
        int? pageLimit = null,
        string? continueToken = null,
        CancellationToken cancellationToken = default
    ) => GetKubernetesClient()
        .CoreV1.ListNamespaceAsync(
            limit: pageLimit,
            continueParameter: continueToken,
            cancellationToken: cancellationToken
        );

    public override async IAsyncEnumerable<WatchEvent<V1Namespace>> GetResourceWatchList(
        string? lastResourceVersion = null,
        int? timeoutSeconds = null,
        [EnumeratorCancellation] CancellationToken cancellationToken = default
    )
    {
        Exception? watchException = null;
        
        IAsyncEnumerable<(WatchEventType, V1Namespace)> watchStream = GetKubernetesClient()
            .CoreV1.WatchListNamespaceAsync(
                resourceVersion: lastResourceVersion,
                allowWatchBookmarks: true,
                timeoutSeconds: timeoutSeconds,
                onError: ex => watchException = ex,
                cancellationToken: cancellationToken
            );
        await foreach ((WatchEventType type, V1Namespace item) in watchStream)
        {
            yield return new WatchEvent<V1Namespace>
            {
                Type = type,
                Object = item,
            };
        }
        
        if (watchException is not null)
            throw watchException;
    }
}
