using k8s;
using k8s.Autorest;
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

    public override Task<HttpOperationResponse<V1NamespaceList>> GetResourceWatchList(
        string? lastResourceVersion = null,
        int? timeoutSeconds = null,
        CancellationToken? cancellationToken = null
    ) => GetKubernetesClient()
        .CoreV1.ListNamespaceWithHttpMessagesAsync(
            watch: true,
            resourceVersion: lastResourceVersion,
            allowWatchBookmarks: true,
            timeoutSeconds: timeoutSeconds,
            cancellationToken: cancellationToken ?? CancellationToken.None
        );
}
