using k8s;
using k8s.Models;
using Microsoft.Extensions.Options;
using TrivyOperator.Dashboard.Application.K8s.Models.WatcherEvents;
using TrivyOperator.Dashboard.Application.K8s.Pipeline;
using TrivyOperator.Dashboard.Application.K8s.Services.Options;
using TrivyOperator.Dashboard.Application.K8s.Services.Watchers.Abstractions;
using TrivyOperator.Dashboard.Domain.K8s.ValueObjects;
using TrivyOperator.Dashboard.Infrastructure.Clients.Metrics.Abstractions;
using TrivyOperator.Dashboard.Infrastructure.K8s.Services.Abstractions;

namespace TrivyOperator.Dashboard.Application.K8s.Services.Watchers;

public class NamespacedWatcher<TKubernetesObjectList, TKubernetesObject>(
    INamespacedResourceService<TKubernetesObject, TKubernetesObjectList> namespacedResourceService,
    IKubernetesBackgroundQueue<TKubernetesObject> backgroundQueue,
    IOptions<WatchersOptions> options,
    IMetricsClient metricsClient,
    ILogger<NamespacedWatcher<TKubernetesObjectList, TKubernetesObject>>
        logger
) : KubernetesWatcher<TKubernetesObjectList, TKubernetesObject>(
    backgroundQueue,
    options,
    metricsClient,
    logger
), INamespacedWatcher
    where TKubernetesObject : class, IKubernetesObject<V1ObjectMeta>, new()
    where TKubernetesObjectList : IKubernetesObject<V1ListMeta>, IItems<TKubernetesObject>
{
    public async Task ReconcileNamespaces(
        ContextName contextName, IReadOnlyList<NamespaceName> newNamespaceNames, CancellationToken ctx = default)
    {
        NamespaceName[] existing =
            [.. Watchers.Where(kvp => kvp.Key.ContextName == contextName).Select(kvp => kvp.Key.NamespaceName),];
        IEnumerable<NamespaceName> toAdd = newNamespaceNames.Except(existing);
        IEnumerable<NamespaceName> toDelete = existing.Except(newNamespaceNames);
        List<Task> tasks =
        [
            .. toDelete.Select(namespaceName => StopWatcher(new WatcherKey(contextName, namespaceName), ctx)),
        ];
        foreach (NamespaceName namespaceName in toAdd)
        {
            StartWatcher(new WatcherKey(contextName, namespaceName), ctx);
        }
        await Task.WhenAll(tasks);
    }

    protected override IAsyncEnumerable<WatchEvent<TKubernetesObject>> GetKubernetesObjectWatchList(
        WatcherKey key,
        string? lastResourceVersion,
        CancellationToken cancellationToken = default
    ) => namespacedResourceService.GetResourceWatchList(
        key.NamespaceName.Value,
        lastResourceVersion,
        GetWatcherRandomTimeout(),
        cancellationToken
    );

    protected override async Task<TKubernetesObjectList> GetInitialResources(
        WatcherKey key,
        string? continueToken,
        CancellationToken cancellationToken = default
    ) => await namespacedResourceService.GetResourceList(
        key.NamespaceName.Value,
        ResourceListPageSize,
        continueToken,
        cancellationToken
    );
}
