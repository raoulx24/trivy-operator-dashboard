using TrivyOperator.Dashboard.Application.KubernetesEventPipeline.Models.WatcherEvents;

namespace TrivyOperator.Dashboard.Application.KubernetesEventPipeline.Services.WatcherRegistries.Abstractions;

public interface INamespacedWatcherRegistry : IKubernetesWatcherRegistry
{
    Task Reconcile(IReadOnlyCollection<WatcherKey> desiredKeys, CancellationToken ctx = default);
}
