using TrivyOperator.Dashboard.Domain.Kubernetes.ValueObjects;

namespace TrivyOperator.Dashboard.Application.Kubernetes.WatcherRegistries.Abstractions;

public interface IKubernetesWatcherRegistry
{
    void StartWatcher(ResourceLocation key, CancellationToken ctx = default);

    Task StopWatcher(ResourceLocation key, CancellationToken ctx = default);

    Task RecreateWatcher(ResourceLocation key, CancellationToken ctx = default);
    
    Type WatchedKubernetesObjectType { get; }
}
