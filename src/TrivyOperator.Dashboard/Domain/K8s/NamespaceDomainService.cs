using k8s;
using k8s.Models;
using TrivyOperator.Dashboard.Domain.K8s.Abstractions;
using TrivyOperator.Dashboard.Domain.K8s.UpstreamAbstractions;

namespace TrivyOperator.Dashboard.Domain.K8s;

public class NamespaceDomainService(IKubernetesClientFactory kubernetesClientFactory, IServiceScopeFactory scopeFactory)
    : ClusterScopedResourceDomainService<V1Namespace, V1NamespaceList>(kubernetesClientFactory, scopeFactory)
{
    public override Task<V1Namespace> GetResource(string resourceName, CancellationToken? cancellationToken = null) =>
        GetKubernetesClient()
            .CoreV1.ReadNamespaceAsync(resourceName, cancellationToken: cancellationToken ?? CancellationToken.None);

    public override Task<V1NamespaceList> GetResourceList(
        int? pageLimit = null,
        string? continueToken = null,
        CancellationToken? cancellationToken = null
    ) => GetKubernetesClient()
        .CoreV1.ListNamespaceAsync(
            limit: pageLimit,
            continueParameter: continueToken,
            cancellationToken: cancellationToken ?? CancellationToken.None
        );

    public override async IAsyncEnumerable<WatchEvent<V1Namespace>> GetResourceWatchList(
        string? lastResourceVersion = null,
        int? timeoutSeconds = null,
        Action<Exception>? onError = null,
        CancellationToken? cancellationToken = null
    )
    {
        IAsyncEnumerable<(WatchEventType, V1Namespace)> watchStream = GetKubernetesClient()
            .CoreV1.WatchListNamespaceAsync(
                resourceVersion: lastResourceVersion,
                allowWatchBookmarks: true,
                timeoutSeconds: timeoutSeconds,
                onError: onError,
                cancellationToken: cancellationToken ?? CancellationToken.None
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
