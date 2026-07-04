using k8s.Models;
using TrivyOperator.Dashboard.Application.K8s.Services.EventDispatchers.Abstractions;
using TrivyOperator.Dashboard.Application.K8s.Services.WatcherEvents;
using TrivyOperator.Dashboard.Application.K8s.Services.WatcherEvents.Abstractions;
using TrivyOperator.Dashboard.Domain.History.NamespaceHistory.Abstractions;
using TrivyOperator.Dashboard.Domain.History.VulnerabilityReportsHistory.ValueObjects;
using TrivyOperator.Dashboard.Domain.K8s.ValueObjects;

namespace TrivyOperator.Dashboard.Application.History.NamespaceHistory.Services;

public sealed class NamespaceHistoryRefresher(
    IServiceScopeFactory scopeFactory,
    ILogger<NamespaceHistoryRefresher> logger
) : IKubernetesEventProcessor<V1Namespace>
{
    private static string ResourceKind => nameof(V1Namespace).ToLowerInvariant();
    public async Task ProcessKubernetesEvent(
        IWatcherEvent<V1Namespace> watcherEvent,
        CancellationToken cancellationToken)
    {
        switch (watcherEvent.WatcherEventType)
        {
            case WatcherEventType.Added:
            case WatcherEventType.Modified:
            case WatcherEventType.InitialAdded:
                await HandleUpsertAsync(watcherEvent, cancellationToken);
                break;
            case WatcherEventType.Deleted:
            case WatcherEventType.Initialized:
            case WatcherEventType.Error:
            case WatcherEventType.Flushed:
            case WatcherEventType.Bookmark:
            case WatcherEventType.WatcherConnected:
                break;
            case WatcherEventType.Unknown:
            default:
                logger.LogWarning(
                    "Unknown event type {eventType} for {kubernetesObjectType}.",
                    watcherEvent.WatcherEventType,
                    ResourceKind);
                break;
        }
    }
    
    private async Task HandleUpsertAsync(
        IWatcherEvent<V1Namespace> watcherEvent,
        CancellationToken cancellationToken)
    {
        V1Namespace? ns = watcherEvent.KubernetesObject;

        if (ns is null)
        {
            logger.LogWarning(
                "HandleUpsertAsync - KubernetesObject is null for {watcherKey} {kubernetesObjectType}. Ignoring",
                watcherEvent.WatcherKey,
                ResourceKind);

            return;
        }

        string? namespaceValue = ns.Metadata?.Name;

        if (string.IsNullOrWhiteSpace(namespaceValue))
        {
            logger.LogWarning(
                "HandleUpsertAsync - Namespace name is null or empty for {watcherKey} {kubernetesObjectType}. Ignoring",
                watcherEvent.WatcherKey,
                ResourceKind);

            return;
        }

        await using AsyncServiceScope scope = scopeFactory.CreateAsyncScope();

        INamespaceHistoryStore store =
            scope.ServiceProvider.GetRequiredService<INamespaceHistoryStore>();

        try
        {
            await store.AddOrUpdateNamespaceAsync(
                new NamespaceName(namespaceValue),
                cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogError(
                ex,
                "Failed to upsert namespace for {kubernetesObjectType} in {watcherKey}.",
                ResourceKind,
                watcherEvent.WatcherKey);
        }
    }
    
    private async Task HandleDeleteAsync(
        IWatcherEvent<V1Namespace> watcherEvent,
        CancellationToken cancellationToken)
    {
        V1Namespace? ns = watcherEvent.KubernetesObject;

        if (ns is null)
        {
            logger.LogWarning(
                "HandleDeleteAsync - KubernetesObject is null for {watcherKey} {kubernetesObjectType}. Ignoring",
                watcherEvent.WatcherKey,
                ResourceKind);

            return;
        }

        string? namespaceValue = ns.Metadata?.Name;

        if (string.IsNullOrWhiteSpace(namespaceValue))
        {
            logger.LogWarning(
                "HandleDeleteAsync - Namespace name is null or empty for {watcherKey} {kubernetesObjectType}. Ignoring",
                watcherEvent.WatcherKey,
                ResourceKind);

            return;
        }

        await using AsyncServiceScope scope = scopeFactory.CreateAsyncScope();

        INamespaceHistoryStore store =
            scope.ServiceProvider.GetRequiredService<INamespaceHistoryStore>();

        try
        {
            await store.DeleteNamespacesAsync(
            [
                new NamespaceName(namespaceValue)
            ], cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogError(
                ex,
                "Failed to delete namespace for {kubernetesObjectType} in {watcherKey}.",
                ResourceKind,
                watcherEvent.WatcherKey);
        }
    }
}
