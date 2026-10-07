using TrivyOperator.Dashboard.Domain.Shared.Abstractions;

namespace TrivyOperator.Dashboard.Application.Kubernetes.WatcherRegistries.Abstractions;

public interface IClusterScopedWatcherRegistry : IKubernetesWatcherRegistry;

public interface IClusterScopedWatcherRegistry<TResource, TKey> : IClusterScopedWatcherRegistry
    where TResource : class, IEntity<TKey>;
