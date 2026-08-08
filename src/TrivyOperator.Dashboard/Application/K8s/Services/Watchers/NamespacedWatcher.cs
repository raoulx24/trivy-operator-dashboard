using k8s;
using k8s.Models;
using Microsoft.Extensions.Options;
using TrivyOperator.Dashboard.Application.K8s.Pipeline;
using TrivyOperator.Dashboard.Application.K8s.Services.Options;
using TrivyOperator.Dashboard.Application.K8s.Services.Watchers.Abstractions;
using TrivyOperator.Dashboard.Domain.K8s.ValueObjects;
using TrivyOperator.Dashboard.Infrastructure.Clients.Abstractions;
using TrivyOperator.Dashboard.Infrastructure.K8s.Services.Abstractions;

namespace TrivyOperator.Dashboard.Application.K8s.Services.Watchers;

public class NamespacedWatcher<TKubernetesObjectList, TKubernetesObject, TBackgroundQueue>(
    INamespacedResourceService<TKubernetesObject, TKubernetesObjectList>
        namespacedResourceService,
    TBackgroundQueue backgroundQueue,
    IOptions<WatchersOptions> options,
    IMetricsClient metricsClient,
    ILogger<NamespacedWatcher<TKubernetesObjectList, TKubernetesObject, TBackgroundQueue>>
        logger
) : KubernetesWatcher<TKubernetesObjectList, TKubernetesObject, TBackgroundQueue>(
    namespacedResourceService,
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
    public async Task ReconcileNamespaces(string[] newNamespaceNames, CancellationToken cancellationToken)
    {
        string[] existingWatcherKeys = Watchers.Select(kvp => kvp.Key).ToArray();
        IEnumerable<string> newWatcherKeys = newNamespaceNames.Except(existingWatcherKeys);
        IEnumerable<string> staleWatcherKeys = existingWatcherKeys.Except(newNamespaceNames);
        List<Task> tasks = [];
        tasks.AddRange(newWatcherKeys.Select(watcherKey => Add(cancellationToken, watcherKey)));
        tasks.AddRange(staleWatcherKeys.Select(watcherKey => Delete(watcherKey, cancellationToken)));
        await Task.WhenAll(tasks);
    }

    protected override IAsyncEnumerable<WatchEvent<TKubernetesObject>> GetKubernetesObjectWatchList(
        string watcherKey,
        string? lastResourceVersion,
        Action<Exception>? onError = null,
        CancellationToken cancellationToken = default
    ) => namespacedResourceService.GetResourceWatchList(
        watcherKey,
        lastResourceVersion,
        GetWatcherRandomTimeout(),
        onError,
        cancellationToken
    );

    protected override async Task<TKubernetesObjectList> GetInitialResources(
        string watcherKey,
        string? continueToken,
        CancellationToken cancellationToken = default
    ) => await namespacedResourceService.GetResourceList(
        watcherKey,
        resourceListPageSize,
        continueToken,
        cancellationToken
    );
}
