using k8s;
using k8s.Autorest;
using k8s.Models;
using TrivyOperator.Dashboard.Domain.Services.Abstractions;
using TrivyOperator.Dashboard.Infrastructure.Abstractions;

namespace TrivyOperator.Dashboard.Domain.Services;

public class NamespaceDomainService(IKubernetesClientFactory kubernetesClientFactory, IKubernetesContextProvider kubernetesContextProvider)
    : ClusterScopedResourceDomainService<V1Namespace, V1NamespaceList>(kubernetesClientFactory, kubernetesContextProvider)
{
    public override Task<V1Namespace> GetResource(
        string resourceName,
        CancellationToken? cancellationToken = null) => kubernetesClient
        .CoreV1.ReadNamespaceAsync(resourceName, cancellationToken: cancellationToken ?? CancellationToken.None);

    public override Task<V1NamespaceList> GetResourceList(
        int? pageLimit = null,
        string? continueToken = null,
        CancellationToken? cancellationToken = null) => kubernetesClient
                .CoreV1.ListNamespaceAsync(
                    limit: pageLimit,
                    continueParameter: continueToken,
                    cancellationToken: cancellationToken ?? CancellationToken.None);

    public override Task<HttpOperationResponse<V1NamespaceList>> GetResourceWatchList(
        string? lastResourceVersion = null,
        int? timeoutSeconds = null,
        CancellationToken? cancellationToken = null) => kubernetesClient
        .CoreV1.ListNamespaceWithHttpMessagesAsync(
            watch: true,
            resourceVersion: lastResourceVersion,
            allowWatchBookmarks: true,
            timeoutSeconds: timeoutSeconds,
            cancellationToken: cancellationToken ?? CancellationToken.None);
}
