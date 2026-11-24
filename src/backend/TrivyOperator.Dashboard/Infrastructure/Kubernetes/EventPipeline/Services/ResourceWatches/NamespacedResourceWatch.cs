using k8s;
using k8s.Models;
using TrivyOperator.Dashboard.Application.Kubernetes.Namespaces.Abstractions;
using TrivyOperator.Dashboard.Domain.Kubernetes.ValueObjects;
using TrivyOperator.Dashboard.Infrastructure.Kubernetes.EventPipeline.Services.ResourceWatches.Abstractions;
using TrivyOperator.Dashboard.Infrastructure.Kubernetes.Services.Abstractions;

namespace TrivyOperator.Dashboard.Infrastructure.Kubernetes.EventPipeline.Services.ResourceWatches;

// a namespaced type, watched either in one namespace or, for a cluster wide scope, in all namespaces;
// cluster wide, the excluded namespaces are filtered on the server (field selector)
public sealed class NamespacedResourceWatch<TKubernetesObjectList, TKubernetesObject>(
    INamespacedResourceService<TKubernetesObject, TKubernetesObjectList> resourceService,
    INamespacePolicyProvider namespacePolicyProvider
) : IKubernetesResourceWatch<TKubernetesObjectList, TKubernetesObject>
    where TKubernetesObject : class, IKubernetesObject<V1ObjectMeta>, new()
    where TKubernetesObjectList : IKubernetesObject<V1ListMeta>, IItems<TKubernetesObject>
{
    public Task<ResourcePage<TKubernetesObject>> GetInitialResources(
        WatcherScope scope,
        string? continueToken,
        int pageSize,
        CancellationToken cancellationToken = default
    ) =>
        scope.IsClusterWide
            ? resourceService.GetAllNamespacesResourcePage(
                pageSize,
                continueToken,
                GetExcludedFieldSelector(scope),
                cancellationToken
            )
            : resourceService.GetResourcePage(
                scope.NamespaceName.Value,
                pageSize,
                continueToken,
                cancellationToken
            );

    public IAsyncEnumerable<WatchEvent<TKubernetesObject>> GetWatchList(
        WatcherScope scope,
        string? resourceVersion,
        int timeoutSeconds,
        CancellationToken cancellationToken = default
    ) =>
        scope.IsClusterWide
            ? resourceService.GetAllNamespacesResourceWatchList(
                resourceVersion,
                timeoutSeconds,
                GetExcludedFieldSelector(scope),
                cancellationToken
            )
            : resourceService.GetResourceWatchList(
                scope.NamespaceName.Value,
                resourceVersion,
                timeoutSeconds,
                cancellationToken
            );

    private string? GetExcludedFieldSelector(WatcherScope scope) =>
        namespacePolicyProvider.Get(scope.ContextName).ExcludedFieldSelector;
}
