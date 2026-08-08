using k8s;
using k8s.Models;
using Microsoft.Extensions.Options;
using TrivyOperator.Dashboard.Application.K8s.Models.WatcherEvents;
using TrivyOperator.Dashboard.Application.K8s.Pipeline;
using TrivyOperator.Dashboard.Application.K8s.Services.Options;
using TrivyOperator.Dashboard.Application.K8s.Services.Watchers.Abstractions;
using TrivyOperator.Dashboard.Domain.K8s.ValueObjects;
using TrivyOperator.Dashboard.Infrastructure.Clients.Abstractions;
using TrivyOperator.Dashboard.Infrastructure.K8s.Services.Abstractions;

namespace TrivyOperator.Dashboard.Application.K8s.Services.Watchers;

public class NamespacedWatcher<TKubernetesObjectList, TKubernetesObject, TBackgroundQueue>(
    INamespacedResourceService<TKubernetesObject, TKubernetesObjectList> namespacedResourceService,
    TBackgroundQueue backgroundQueue,
    IOptions<WatchersOptions> options,
    IMetricsClient metricsClient,
    ILogger<NamespacedWatcher<TKubernetesObjectList, TKubernetesObject, TBackgroundQueue>>
        logger
) : KubernetesWatcher<TKubernetesObjectList, TKubernetesObject, TBackgroundQueue>(
    backgroundQueue,
    options,
    metricsClient,
    logger
), INamespacedWatcher<TKubernetesObject>
    where TKubernetesObject : class, IKubernetesObject<V1ObjectMeta>, new()
    where TKubernetesObjectList : IKubernetesObject<V1ListMeta>, IItems<TKubernetesObject>
    where TBackgroundQueue : IKubernetesBackgroundQueue<TKubernetesObject>
{
    // TODO: new for ns cleanup
    public async Task ReconcileNamespaces(ContextName contextName, NamespaceName[] newNamespaceNames, CancellationToken ctx = default)
    {
        NamespaceName[] existing = Watchers.Where(kvp => kvp.Key.ContextName == contextName).Select(kvp => kvp.Key.NamespaceName).ToArray();
        IEnumerable<NamespaceName> toAdd = newNamespaceNames.Except(existing);
        IEnumerable<NamespaceName> toDelete = existing.Except(newNamespaceNames);
        List<Task> tasks = [];
        tasks.AddRange(toAdd.Select(namespaceName => Add(new WatcherKey(contextName, namespaceName), ctx)));
        tasks.AddRange(toDelete.Select(namespaceName => Delete(new WatcherKey(contextName, namespaceName), ctx)));
        await Task.WhenAll(tasks);
    }

    protected override IAsyncEnumerable<WatchEvent<TKubernetesObject>> GetKubernetesObjectWatchList(
        WatcherKey key,
        string? lastResourceVersion,
        Action<Exception>? onError = null,
        CancellationToken cancellationToken = default
    ) => namespacedResourceService.GetResourceWatchList(
        key.NamespaceName.Value,
        lastResourceVersion,
        GetWatcherRandomTimeout(),
        onError,
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
