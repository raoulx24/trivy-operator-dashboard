using TrivyOperator.Dashboard.Domain.Kubernetes.ValueObjects;

namespace TrivyOperator.Dashboard.Application.Kubernetes.WatcherRegistries.Abstractions;

public interface INamespacedWatcherRegistry : IKubernetesWatcherRegistry
{
    Task Reconcile(IReadOnlyCollection<ResourceLocation> desiredKeys, CancellationToken ctx = default);
}
