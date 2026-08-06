using k8s;
using k8s.Models;
using System.Runtime.CompilerServices;
using TrivyOperator.Dashboard.Domain.K8s.ValueObjects;
using TrivyOperator.Dashboard.Infrastructure.K8s.ClientFactory.Abstractions;

namespace TrivyOperator.Dashboard.Infrastructure.K8s.Services;

public class NamespaceService(IKubernetesClientFactory kubernetesClientFactory, IKubernetesContextResolver contextResolver)
    : ClusterScopedResourceService<V1Namespace, V1NamespaceList>(kubernetesClientFactory, contextResolver)
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
        Action<Exception>? onError = null,
        [EnumeratorCancellation] CancellationToken cancellationToken = default
    )
    {
        IAsyncEnumerable<(WatchEventType, V1Namespace)> watchStream = GetKubernetesClient()
            .CoreV1.WatchListNamespaceAsync(
                resourceVersion: lastResourceVersion,
                allowWatchBookmarks: true,
                timeoutSeconds: timeoutSeconds,
                onError: onError,
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
    }
}
